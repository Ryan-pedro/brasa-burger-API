namespace Brasa.Application.Shared.Outbox;

public interface IOutboxStore
{
    Task<int> ProcessPendingAsync(Func<OutboxMessage, CancellationToken, Task> deliver, CancellationToken cancellationToken);
}
