using Brasa.Application.Shared.Jobs;
using Brasa.Application.Shared.Persistence;
using Brasa.Domain.Ordering;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Privacy;

public sealed class RemoveExpiredPersonalDataJob(IAppDbContext db, PersonalDataRemoval removal, TimeProvider clock) : IJob
{
    public const int BatchSize = 200;

    private static readonly OrderStatus[] FinalStatuses = Enum.GetValues<OrderStatus>().Where(OrderTransitions.IsFinal).ToArray();

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var limit = clock.GetUtcNow() - PersonalDataRetention.Period;

        while (true)
        {
            var batch = await db.Orders
                .Where(order => order.PersonalDataRemovedAt == null && order.CreatedAt < limit && FinalStatuses.Contains(order.Status))
                .OrderBy(order => order.CreatedAt)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

            if (batch.Count == 0)
            {
                return;
            }

            await removal.RemoveAsync(batch, cancellationToken);
        }
    }
}
