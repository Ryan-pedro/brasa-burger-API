using Brasa.Application.Shared.Jobs;
using Brasa.Application.Shared.Persistence;
using Brasa.Domain.Printing;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Printing.Jobs;

public sealed class RecoverPrintJobsJob(IAppDbContext db, TimeProvider clock) : IJob
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();

        var stuck = await db.PrintJobs
            .Where(job => (job.Status == PrintJobStatus.Leased && job.LeasedUntil <= now)
                || (job.Status == PrintJobStatus.Pending && job.ExpiresAt <= now))
            .ToListAsync(cancellationToken);

        foreach (var job in stuck)
        {
            if (!job.ExpireLease(now))
            {
                job.AbandonIfExpired(now);
            }
        }

        if (stuck.Count > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
