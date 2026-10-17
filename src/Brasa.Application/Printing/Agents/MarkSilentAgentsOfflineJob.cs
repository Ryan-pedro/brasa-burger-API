using Brasa.Application.Shared.Jobs;
using Brasa.Application.Shared.Persistence;
using Brasa.Domain.Printing;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Printing.Agents;

public sealed class MarkSilentAgentsOfflineJob(IAppDbContext db, TimeProvider clock) : IJob
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var silentSince = now - PrintAgent.OfflineAfter;

        var silent = await db.PrintAgents
            .Where(agent => agent.IsOnline && agent.LastSeenAt <= silentSince)
            .ToListAsync(cancellationToken);

        foreach (var agent in silent)
        {
            agent.MarkOffline();
        }

        if (silent.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
