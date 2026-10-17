using Brasa.Application.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Printing.Agents;

public static class AgentConfigQuery
{
    public static async Task<AgentConfigResponse> AgentConfigAsync(this IAppDbContext db, Guid agentId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var printers = await db.Printers
            .AsNoTracking()
            .Where(printer => printer.AgentId == agentId)
            .OrderBy(printer => printer.Name)
            .ToListAsync(cancellationToken);

        return new AgentConfigResponse(now, AgentProtocol.Current, printers.Select(AgentPrinterConfig.From).ToList());
    }
}
