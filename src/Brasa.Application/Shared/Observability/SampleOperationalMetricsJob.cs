using Brasa.Application.Shared.Jobs;
using Brasa.Application.Shared.Persistence;
using Brasa.Domain.Payments;
using Brasa.Domain.Printing;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Shared.Observability;

public sealed class SampleOperationalMetricsJob(IAppDbContext db, BrasaMetrics metrics, TimeProvider clock) : IJob
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();

        var oldestOutbox = await db.OutboxMessages
            .Where(message => message.ProcessedAt == null)
            .MinAsync(message => (DateTimeOffset?)message.CreatedAt, cancellationToken);

        var pendingPrintJobs = db.PrintJobs.Where(job => job.Status == PrintJobStatus.Pending);
        var pendingPrintCount = await pendingPrintJobs.LongCountAsync(cancellationToken);
        var oldestPrintJob = await pendingPrintJobs.MinAsync(job => (DateTimeOffset?)job.CreatedAt, cancellationToken);

        var onlineAgents = await db.PrintAgents.LongCountAsync(agent => agent.IsOnline && agent.RevokedAt == null, cancellationToken);
        var waitingPix = await db.Payments.LongCountAsync(payment => payment.Method == PaymentMethod.PixOnline && payment.Status == PaymentStatus.Pending, cancellationToken);

        metrics.Record(new OperationalSnapshot(
            AgeInSeconds(oldestOutbox, now),
            pendingPrintCount,
            AgeInSeconds(oldestPrintJob, now),
            onlineAgents,
            waitingPix));
    }

    private static double AgeInSeconds(DateTimeOffset? since, DateTimeOffset now) =>
        since is null ? 0 : Math.Max(0, (now - since.Value).TotalSeconds);
}
