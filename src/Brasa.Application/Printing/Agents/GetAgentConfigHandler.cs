using Brasa.Application.Shared.Persistence;

namespace Brasa.Application.Printing.Agents;

public sealed class GetAgentConfigHandler(IAppDbContext db, CurrentAgent currentAgent, TimeProvider clock)
{
    public Task<AgentConfigResponse> HandleAsync(CancellationToken cancellationToken) =>
        db.AgentConfigAsync(currentAgent.RequireAgentId(), clock.GetUtcNow(), cancellationToken);
}
