using Brasa.Application.Shared.Jobs;
using Brasa.Application.Shared.Realtime;

namespace Brasa.Application.Shared.Outbox;

public sealed class DispatchOutboxJob(IOutboxStore outbox, IRealtimeNotifier notifier) : IJob
{
    public Task RunAsync(CancellationToken cancellationToken) =>
        outbox.ProcessPendingAsync(
            (message, token) => notifier.NotifyAsync(message.Channel, new RealtimeNotice(message.Type, message.SubjectId), token),
            cancellationToken);
}
