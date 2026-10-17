using Brasa.Application.Shared.Events;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Realtime;
using Brasa.Domain.Printing;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Printing;

public sealed class PrintingNoticeReactions(IAppDbContext db, TimeProvider clock)
    : IDomainEventHandler<PrintJobChanged>, IDomainEventHandler<PrinterStatusChanged>, IDomainEventHandler<PrintAgentPresenceChanged>
{
    public async Task HandleAsync(PrintJobChanged domainEvent, CancellationToken cancellationToken)
    {
        var job = domainEvent.Job;
        var now = clock.GetUtcNow();

        db.QueueRealtimeNotice(RealtimeChannels.Ops, RealtimeNoticeTypes.PrintingChanged, job.Id, now);

        if (job.TicketId is { } ticketId)
        {
            db.QueueRealtimeNotice(RealtimeChannels.Kitchen, RealtimeNoticeTypes.TicketChanged, ticketId, now);
        }

        if (job.Status == PrintJobStatus.Pending)
        {
            var agentId = db.Printers.Local.FirstOrDefault(printer => printer.Id == job.PrinterId)?.AgentId
                ?? await db.Printers.Where(printer => printer.Id == job.PrinterId).Select(printer => printer.AgentId).FirstAsync(cancellationToken);

            db.QueueRealtimeNotice(RealtimeChannels.Agent, RealtimeNoticeTypes.PrintJobsAvailable, agentId, now);
        }
    }

    public Task HandleAsync(PrinterStatusChanged domainEvent, CancellationToken cancellationToken) =>
        QueuePrintingChanged(domainEvent.Printer.Id);

    public Task HandleAsync(PrintAgentPresenceChanged domainEvent, CancellationToken cancellationToken) =>
        QueuePrintingChanged(domainEvent.Agent.Id);

    private Task QueuePrintingChanged(Guid subjectId)
    {
        var now = clock.GetUtcNow();
        db.QueueRealtimeNotice(RealtimeChannels.Kitchen, RealtimeNoticeTypes.PrintingChanged, subjectId, now);
        db.QueueRealtimeNotice(RealtimeChannels.Ops, RealtimeNoticeTypes.PrintingChanged, subjectId, now);
        return Task.CompletedTask;
    }
}
