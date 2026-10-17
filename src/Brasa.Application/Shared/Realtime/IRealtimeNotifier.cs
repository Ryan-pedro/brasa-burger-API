namespace Brasa.Application.Shared.Realtime;

public interface IRealtimeNotifier
{
    Task NotifyAsync(string channel, RealtimeNotice notice, CancellationToken cancellationToken);
}
