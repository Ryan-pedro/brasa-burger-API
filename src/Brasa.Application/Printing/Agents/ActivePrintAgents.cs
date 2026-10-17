using Brasa.Application.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Printing.Agents;

public sealed class ActivePrintAgents(IAppDbContext db)
{
    public Task<bool> IsActiveAsync(Guid agentId, CancellationToken cancellationToken) =>
        db.PrintAgents.AnyAsync(agent => agent.Id == agentId && agent.RevokedAt == null, cancellationToken);
}
