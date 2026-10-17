using Brasa.Api.Auth;
using Brasa.Application.Printing.Agents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Brasa.Api.Realtime;

[Authorize(AgentAuth.Scheme)]
internal sealed class AgentHub(ActivePrintAgents activeAgents) : Hub
{
    public const string Path = "/api/v1/hubs/agent";

    public override async Task OnConnectedAsync()
    {
        if (!AgentAuth.TryReadAgentId(Context.User!, out var agentId) || !await activeAgents.IsActiveAsync(agentId, Context.ConnectionAborted))
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupOf(agentId), Context.ConnectionAborted);
        await base.OnConnectedAsync();
    }

    public static string GroupOf(Guid agentId) => $"agent:{agentId}";
}
