using System.Collections.Concurrent;
using Brasa.Application.Shared.Outbox;

namespace Brasa.IntegrationTests;

public sealed class RecordingNotifier : IRealtimeNotifier
{
    private readonly ConcurrentQueue<(string Channel, RealtimeNotice Notice)> _sent = new();

    public IReadOnlyCollection<(string Channel, RealtimeNotice Notice)> Sent => _sent;

    public Task NotifyAsync(string channel, RealtimeNotice notice, CancellationToken cancellationToken)
    {
        _sent.Enqueue((channel, notice));
        return Task.CompletedTask;
    }
}
