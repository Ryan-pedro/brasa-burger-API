using Brasa.Application.Shared.Idempotency;
using Brasa.Application.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Shared.Jobs;

public sealed class PurgeExpiredRecordsJob(IAppDbContext db, TimeProvider clock) : IJob
{
    public static readonly TimeSpan OutboxRetention = TimeSpan.FromDays(7);

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var idempotencyLimit = now - IdempotencyRecord.Retention;
        var outboxLimit = now - OutboxRetention;

        await db.IdempotencyRecords
            .Where(record => record.CreatedAt < idempotencyLimit)
            .ExecuteDeleteAsync(cancellationToken);

        await db.OutboxMessages
            .Where(message => message.ProcessedAt != null && message.ProcessedAt < outboxLimit)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
