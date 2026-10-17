using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Jobs;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.StoreSettings.Shared;
using Brasa.Domain.Ordering;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Ordering.Management;

public sealed class RejectStaleOrdersJob(IAppDbContext db, AuditTrail audit, TimeProvider clock) : IJob
{
    public const string Reason = "A loja não respondeu no prazo.";

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var store = await db.CurrentStoreAsync(cancellationToken);
        if (store is null)
        {
            return;
        }

        var now = clock.GetUtcNow();
        var deadline = now.AddMinutes(-store.AcceptanceTimeoutMinutes);

        var stale = await db.Orders
            .Where(order => order.Status == OrderStatus.Placed && order.UpdatedAt <= deadline)
            .ToListAsync(cancellationToken);

        foreach (var order in stale)
        {
            order.Reject(Reason, now);
            audit.Record("order.reject", order.Id, new { reason = Reason });
        }

        if (stale.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
