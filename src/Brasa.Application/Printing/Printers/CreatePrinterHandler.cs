using Brasa.Application.Printing.Agents;
using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Printing;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Printing.Printers;

public sealed class CreatePrinterHandler(IAppDbContext db, AuditTrail audit)
{
    public async Task<Result<PrinterResponse>> HandleAsync(Guid agentId, PrinterRequest request, CancellationToken cancellationToken)
    {
        var agent = await db.PrintAgents.AsNoTracking().FirstOrDefaultAsync(a => a.Id == agentId, cancellationToken);
        if (agent is null)
        {
            return PrintingErrors.AgentNotFound;
        }

        if (agent.IsRevoked)
        {
            return PrintingErrors.AgentRevoked;
        }

        var errors = new FieldErrors();
        var valid = PrinterRules.Validate(request, errors);
        if (valid is null)
        {
            return errors.ToError();
        }

        var printer = new Printer(agentId, valid.Name, valid.PaperWidth, valid.Address);
        db.Printers.Add(printer);

        var isFirstPrinter = !await db.Printers.AnyAsync(cancellationToken);
        await db.ApplyFlagsAsync(printer, request with { IsDefault = request.IsDefault ?? (isFirstPrinter && request.IsActive != false) }, cancellationToken);

        audit.Record("printing.printer_created", printer.Id, new { printer.Name, agentId });
        await db.SaveChangesAsync(cancellationToken);

        return PrinterResponse.From(printer);
    }
}
