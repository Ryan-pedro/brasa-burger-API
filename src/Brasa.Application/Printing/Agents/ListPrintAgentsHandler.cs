using Brasa.Application.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Printing.Agents;

public sealed class ListPrintAgentsHandler(IAppDbContext db)
{
    public async Task<IReadOnlyList<PrintAgentResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        var agents = await db.PrintAgents.AsNoTracking().OrderBy(agent => agent.Name).ToListAsync(cancellationToken);
        var printers = (await db.Printers.AsNoTracking().ToListAsync(cancellationToken)).ToLookup(printer => printer.AgentId);

        return agents.Select(agent => PrintAgentResponse.From(agent, printers[agent.Id])).ToList();
    }
}
