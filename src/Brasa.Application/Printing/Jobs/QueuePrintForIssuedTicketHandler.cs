using Brasa.Application.Printing.Layouts;
using Brasa.Application.Printing.Printers;
using Brasa.Application.Shared.Events;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.StoreSettings.Shared;
using Brasa.Domain.Kitchen;
using Brasa.Domain.Printing;

namespace Brasa.Application.Printing.Jobs;

public sealed class QueuePrintForIssuedTicketHandler(IAppDbContext db, TimeProvider clock) : IDomainEventHandler<TicketIssued>
{
    public async Task HandleAsync(TicketIssued domainEvent, CancellationToken cancellationToken)
    {
        var printer = await db.DefaultPrinterAsync(cancellationToken);
        var store = await db.CurrentStoreAsync(cancellationToken);
        if (printer is null || store is null)
        {
            return;
        }

        var ticket = domainEvent.Ticket;
        var document = KitchenTicketLayout.Build(ticket, store.Name, store.ToStoreTime(ticket.IssuedAt), isCopy: false);

        db.PrintJobs.Add(new PrintJob(printer.Id, ticket.Id, PrintJobReason.Automatic, document, clock.GetUtcNow()));
    }
}
