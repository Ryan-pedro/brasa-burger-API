using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Printing;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Printing.Agents;

public sealed class RevokePrintAgentHandler(IAppDbContext db, AuditTrail audit, TimeProvider clock)
{
    public async Task<Result<PrintAgentResponse>> HandleAsync(Guid agentId, CancellationToken cancellationToken)
    {
        var agent = await db.PrintAgents.FirstOrDefaultAsync(a => a.Id == agentId, cancellationToken);
        if (agent is null)
        {
            return PrintingErrors.AgentNotFound;
        }

        if (agent.IsRevoked)
        {
            return PrintingErrors.AgentRevoked;
        }

        var now = clock.GetUtcNow();
        agent.Revoke(now);

        var printers = await db.Printers.Where(printer => printer.AgentId == agentId).ToListAsync(cancellationToken);
        var printerIds = printers.Select(printer => printer.Id).ToList();
        var openJobs = await db.PrintJobs
            .Where(job => printerIds.Contains(job.PrinterId) && (job.Status == PrintJobStatus.Pending || job.Status == PrintJobStatus.Leased))
            .ToListAsync(cancellationToken);

        foreach (var job in openJobs)
        {
            job.Cancel(now);
        }

        foreach (var printer in printers)
        {
            printer.Deactivate();
        }

        audit.Record("printing.agent_revoked", agent.Id);
        await db.SaveChangesAsync(cancellationToken);

        return PrintAgentResponse.From(agent, printers);
    }
}
