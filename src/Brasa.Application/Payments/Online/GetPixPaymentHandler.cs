using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Ordering;
using Brasa.Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Payments.Online;

public sealed class GetPixPaymentHandler(IAppDbContext db, IPixGateway gateway, TimeProvider clock)
{
    public async Task<Result<PixPaymentResponse>> HandleAsync(string token, CancellationToken cancellationToken)
    {
        if (token.Length != Order.TrackingTokenLength)
        {
            return PaymentErrors.PixPaymentNotFound;
        }

        var order = await db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.TrackingToken == token, cancellationToken);
        if (order is not { PaymentMethod: PaymentMethod.PixOnline })
        {
            return PaymentErrors.PixPaymentNotFound;
        }

        var payment = await db.Payments.FirstAsync(p => p.OrderId == order.Id, cancellationToken);
        if (payment.Status != PaymentStatus.Pending || payment.HasCharge || payment.IsDueAt(clock.GetUtcNow()))
        {
            return PixPaymentResponse.From(payment);
        }

        PixCharge charge;
        try
        {
            charge = await gateway.CreateChargeAsync(
                new PixChargeRequest(payment.Id, payment.Amount, payment.ExpiresAt!.Value, $"Pedido {order.Number}"),
                cancellationToken);
        }
        catch (PixGatewayException)
        {
            return PaymentErrors.PixUnavailable;
        }

        payment.AttachCharge(gateway.Provider, charge.ChargeId, charge.CopyPaste);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            payment = await db.Payments.AsNoTracking().FirstAsync(p => p.OrderId == order.Id, cancellationToken);
        }

        return PixPaymentResponse.From(payment);
    }
}
