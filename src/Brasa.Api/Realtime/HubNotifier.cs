using Brasa.Application.Shared.Realtime;
using Microsoft.AspNetCore.SignalR;

namespace Brasa.Api.Realtime;

internal sealed class HubNotifier(IHubContext<KitchenHub> kitchen, IHubContext<OpsHub> ops, IHubContext<AgentHub> agents) : IRealtimeNotifier
{
    public const string Method = "changed";

    public Task NotifyAsync(string channel, RealtimeNotice notice, CancellationToken cancellationToken) => channel switch
    {
        RealtimeChannels.Kitchen => kitchen.Clients.All.SendAsync(Method, notice, cancellationToken),
        RealtimeChannels.Ops => ops.Clients.All.SendAsync(Method, notice, cancellationToken),
        RealtimeChannels.Agent => agents.Clients.Group(AgentHub.GroupOf(notice.Id)).SendAsync(Method, notice, cancellationToken),
        _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, "Canal de tempo real desconhecido."),
    };
}
