using Brasa.Application.Shared.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Infrastructure.Persistence;

internal sealed class OutboxStore(AppDbContext db, TimeProvider clock) : IOutboxStore
{
    private const int BatchSize = 50;

    public async Task<int> ProcessPendingAsync(Func<OutboxMessage, CancellationToken, Task> deliver, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var messages = await db.OutboxMessages
            .FromSql($"""
                SELECT * FROM shared.outbox_messages
                WHERE processed_at IS NULL AND attempts < {OutboxMessage.MaxAttempts}
                ORDER BY created_at
                LIMIT {BatchSize}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await deliver(message, cancellationToken);
                message.MarkProcessed(clock.GetUtcNow());
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                message.MarkFailed(exception.Message);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return messages.Count;
    }
}
