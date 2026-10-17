using Brasa.Application.Printing.Layouts;
using Brasa.Application.Printing.Printers;
using Brasa.Application.Shared.Events;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.StoreSettings.Shared;
using Brasa.Domain.Kitchen;
using Brasa.Domain.Printing;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Printing.Jobs;

public sealed class CancelPrintsForVoidedTicketHandler(IAppDbContext db, TimeProvider clock) : IDomainEventHandler<TicketVoided>
{
    public async Task HandleAsync(TicketVoided domainEvent, CancellationToken cancellationToken)
    {
        var ticket = domainEvent.Ticket;
        var now = clock.GetUtcNow();

        var jobs = await db.PrintJobs
            .Where(job => job.TicketId == ticket.Id && job.Reason != PrintJobReason.CancellationNotice)
            .ToListAsync(cancellationToken);

        var kitchenMaySeePaper = jobs.Any(job => job.MayHaveBeenPrinted);

        foreach (var job in jobs)
        {
            job.Cancel(now);
        }

        var printer = await db.DefaultPrinterAsync(cancellationToken);
        var store = await db.CurrentStoreAsync(cancellationToken);
        if (!kitchenMaySeePaper || printer is null || store is null)
        {
            return;
        }

        var notice = CancellationNoticeLayout.Build(ticket, store.Name, store.ToStoreTime(now));
        db.PrintJobs.Add(new PrintJob(printer.Id, ticket.Id, PrintJobReason.CancellationNotice, notice, now));
    }
}
