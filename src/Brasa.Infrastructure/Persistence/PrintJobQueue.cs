using Brasa.Application.Printing.Jobs;
using Brasa.Domain.Printing;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Infrastructure.Persistence;

internal sealed class PrintJobQueue(AppDbContext db) : IPrintJobQueue
{
    public async Task<IReadOnlyList<PrintJob>> LeaseAsync(Guid agentId, int maxJobs, DateTimeOffset now, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        var jobs = await db.PrintJobs
            .FromSql($"""
                SELECT j.*, j.xmin FROM printing.jobs j
                JOIN printing.printers p ON p.id = j.printer_id
                WHERE p.agent_id = {agentId}
                  AND p.is_active
                  AND j.status = 'Pending'
                  AND j.next_attempt_at <= {now}
                  AND (j.expires_at IS NULL OR j.expires_at > {now})
                ORDER BY j.created_at
                LIMIT {maxJobs}
                FOR UPDATE OF j SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);

        foreach (var job in jobs)
        {
            job.Lease(agentId, now);
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return jobs;
    }
}
