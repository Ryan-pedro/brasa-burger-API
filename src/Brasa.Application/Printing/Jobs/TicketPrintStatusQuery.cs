using Brasa.Application.Shared.Persistence;
using Brasa.Domain.Printing;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Printing.Jobs;

public static class TicketPrintStatusQuery
{
    public static async Task<IReadOnlyDictionary<Guid, string>> LatestPrintStatusByTicketAsync(
        this IAppDbContext db,
        IReadOnlyCollection<Guid> ticketIds,
        CancellationToken cancellationToken)
    {
        var jobs = await db.PrintJobs
            .AsNoTracking()
            .Where(job => job.TicketId != null && ticketIds.Contains(job.TicketId.Value) && job.Reason != PrintJobReason.CancellationNotice)
            .Select(job => new { TicketId = job.TicketId!.Value, job.Id, job.Status })
            .ToListAsync(cancellationToken);

        return jobs
            .GroupBy(job => job.TicketId)
            .ToDictionary(group => group.Key, group => group.MaxBy(job => job.Id)!.Status.ToString());
    }
}
