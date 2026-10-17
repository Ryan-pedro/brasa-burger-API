using Brasa.Application.Kitchen.Tickets;
using Brasa.Application.Printing.Layouts;
using Brasa.Application.Printing.Printers;
using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Application.StoreSettings.Shared;
using Brasa.Domain.Printing;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Printing.Jobs;

public sealed class ReprintTicketHandler(IAppDbContext db, AuditTrail audit, TimeProvider clock)
{
    public async Task<Result<PrintJobResponse>> HandleAsync(Guid ticketId, CancellationToken cancellationToken)
    {
        var ticket = await db.KitchenTickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);
        if (ticket is null)
        {
            return TicketErrors.NotFound;
        }

        var printer = await db.DefaultPrinterAsync(cancellationToken);
        var store = await db.CurrentStoreAsync(cancellationToken);
        if (printer is null || store is null)
        {
            return PrintingErrors.NoDefaultPrinter;
        }

        var document = KitchenTicketLayout.Build(ticket, store.Name, store.ToStoreTime(ticket.IssuedAt), isCopy: true);
        var job = new PrintJob(printer.Id, ticket.Id, PrintJobReason.Reprint, document, clock.GetUtcNow());

        db.PrintJobs.Add(job);
        audit.Record("ticket.reprinted", ticket.Id, new { orderNumber = ticket.OrderNumber, printJobId = job.Id });
        await db.SaveChangesAsync(cancellationToken);

        return PrintJobResponse.From(job);
    }
}
