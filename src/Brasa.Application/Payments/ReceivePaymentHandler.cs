using Brasa.Application.Ordering.Management;
using Brasa.Application.Ordering.Shared;
using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Realtime;
using Brasa.Application.Shared.Results;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Payments;

public sealed class ReceivePaymentHandler(IAppDbContext db, AuditTrail audit, TimeProvider clock)
{
    public async Task<Result<OrderDetailResponse>> HandleAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        if (order is null)
        {
            return OrderErrors.NotFound;
        }

        var payment = await db.Payments.FirstAsync(p => p.OrderId == orderId, cancellationToken);
        if (!OrderActions.Available(order, payment).Contains(OrderActions.ReceivePayment))
        {
            return OrderErrors.InvalidTransition;
        }

        var now = clock.GetUtcNow();
        payment.MarkPaid(now);
        audit.Record("payment.received", payment.Id, new { orderId });
        db.QueueRealtimeNotice(RealtimeChannels.Ops, RealtimeNoticeTypes.OrderChanged, orderId, now);
        await db.SaveChangesAsync(cancellationToken);

        return await db.DetailAsync(order, cancellationToken);
    }
}
