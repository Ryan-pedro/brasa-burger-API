using Brasa.Application.Shared.Persistence;
using Brasa.Domain.Printing;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Printing.Printers;

public static class PrinterQueries
{
    public static async Task<Printer?> DefaultPrinterAsync(this IAppDbContext db, CancellationToken cancellationToken)
    {
        var activeAgentIds = db.PrintAgents.Where(agent => agent.RevokedAt == null).Select(agent => agent.Id);

        return await db.Printers
            .AsNoTracking()
            .FirstOrDefaultAsync(printer => printer.IsDefault && printer.IsActive && activeAgentIds.Contains(printer.AgentId), cancellationToken);
    }
}
