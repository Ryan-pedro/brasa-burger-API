using Brasa.Application.Ordering.Checkout;
using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Observability;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Realtime;
using Brasa.Application.StoreSettings.Shared;
using Brasa.Domain.Ordering;
using Brasa.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Brasa.Application.Payments.Online;

public sealed class PixPaymentSettlement(
    IAppDbContext db,
    IPixGateway gateway,
    AuditTrail audit,
    BrasaMetrics metrics,
    ILogger<PixPaymentSettlement> logger,
    TimeProvider clock)
{
    public async Task SettleAsync(Payment payment, CancellationToken cancellationToken)
    {
        var order = await db.Orders.FirstAsync(o => o.Id == payment.OrderId, cancellationToken);
        var now = clock.GetUtcNow();

        if (!payment.HasCharge)
        {
            ExpireIfDue(payment, order, now);
            return;
        }

        var charge = await gateway.GetChargeAsync(payment.ChargeId!, cancellationToken);
        switch (charge.State)
        {
            case PixChargeState.Paid:
                await ApplyPaidChargeAsync(payment, order, charge, now, cancellationToken);
                break;
            case PixChargeState.Expired or PixChargeState.Cancelled:
                Expire(payment, order, now);
                break;
            default:
                ExpireIfDue(payment, order, now);
                break;
        }
    }

    private async Task ApplyPaidChargeAsync(Payment payment, Order order, PixChargeStatus charge, DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (payment.Status is not (PaymentStatus.Pending or PaymentStatus.Expired))
        {
            return;
        }

        if (charge.PaidAmount != payment.Amount || charge.Currency != PixPaymentPolicy.Currency)
        {
            payment.MarkAmountMismatch();
            if (order.CanMoveTo(OrderStatus.PaymentExpired))
            {
                order.RefusePaymentWithWrongAmount(now);
            }

            Alert(PaymentAlertKind.AmountMismatch, payment, order, new { expected = payment.Amount, received = charge.PaidAmount, charge.Currency }, now);
            return;
        }

        if (payment.Status == PaymentStatus.Expired)
        {
            payment.MarkPaidLate(now);
            Alert(PaymentAlertKind.PaidAfterExpiration, payment, order, new { payment.Amount }, now);
            return;
        }

        payment.MarkPaid(now);
        order.ConfirmPayment(now);
        audit.Record("payment.pix_confirmed", payment.Id, new { orderId = order.Id, payment.Amount });

        var store = await db.CurrentStoreAsync(cancellationToken);
        if (store is not null)
        {
            AutomaticAcceptance.ApplyTo(order, store, now);
        }
    }

    private static void ExpireIfDue(Payment payment, Order order, DateTimeOffset now)
    {
        if (payment.IsDueAt(now))
        {
            Expire(payment, order, now);
        }
    }

    private static void Expire(Payment payment, Order order, DateTimeOffset now)
    {
        if (payment.Status != PaymentStatus.Pending)
        {
            return;
        }

        payment.Expire();
        if (order.CanMoveTo(OrderStatus.PaymentExpired))
        {
            order.ExpirePayment(now);
        }
    }

    private void Alert(PaymentAlertKind kind, Payment payment, Order order, object details, DateTimeOffset now)
    {
        logger.LogWarning("Pagamento do pedido {OrderNumber} exige atenção: {PaymentAlert}.", order.Number, kind);
        metrics.PaymentAlert(kind.ToString());
        audit.Record($"payment.alert.{kind}", payment.Id, details);
        db.QueueRealtimeNotice(RealtimeChannels.Ops, RealtimeNoticeTypes.OrderChanged, order.Id, now);
    }
}
