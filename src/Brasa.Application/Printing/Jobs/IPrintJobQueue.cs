using Brasa.Domain.Printing;

namespace Brasa.Application.Printing.Jobs;

public interface IPrintJobQueue
{
    Task<IReadOnlyList<PrintJob>> LeaseAsync(Guid agentId, int maxJobs, DateTimeOffset now, CancellationToken cancellationToken);
}
