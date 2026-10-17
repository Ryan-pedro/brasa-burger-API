using Brasa.Application.Printing.Agents;
using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Printing.Printers;

public sealed class UpdatePrinterHandler(IAppDbContext db, AuditTrail audit)
{
    public async Task<Result<PrinterResponse>> HandleAsync(Guid printerId, PrinterRequest request, CancellationToken cancellationToken)
    {
        var printer = await db.Printers.FirstOrDefaultAsync(p => p.Id == printerId, cancellationToken);
        if (printer is null)
        {
            return PrintingErrors.PrinterNotFound;
        }

        var errors = new FieldErrors();
        var valid = PrinterRules.Validate(request, errors);
        if (valid is null)
        {
            return errors.ToError();
        }

        if (await db.PrintAgents.AnyAsync(agent => agent.Id == printer.AgentId && agent.RevokedAt != null, cancellationToken))
        {
            return PrintingErrors.AgentRevoked;
        }

        printer.Change(valid.Name, valid.PaperWidth, valid.Address);
        await db.ApplyFlagsAsync(printer, request, cancellationToken);

        audit.Record("printing.printer_changed", printer.Id, request);
        await db.SaveChangesAsync(cancellationToken);

        return PrinterResponse.From(printer);
    }
}
