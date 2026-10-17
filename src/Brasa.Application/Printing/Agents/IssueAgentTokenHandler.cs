using Brasa.Application.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Printing.Agents;

public sealed class IssueAgentTokenHandler(IAppDbContext db, IAgentTokens tokens, TimeProvider clock)
{
    public async Task<AgentTokenResponse?> HandleAsync(AgentTokenRequest request, CancellationToken cancellationToken)
    {
        if (request.AgentId is not { } agentId || string.IsNullOrEmpty(request.Secret))
        {
            return null;
        }

        var agent = await db.PrintAgents.AsNoTracking().FirstOrDefaultAsync(a => a.Id == agentId, cancellationToken);
        return agent is not null && agent.HasSecret(request.Secret) ? tokens.Issue(agent.Id, clock.GetUtcNow()) : null;
    }
}
