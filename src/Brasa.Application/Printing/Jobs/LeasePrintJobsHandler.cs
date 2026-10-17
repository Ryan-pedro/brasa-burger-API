using Brasa.Application.Printing.Agents;
using Brasa.Application.Shared.Results;

namespace Brasa.Application.Printing.Jobs;

public sealed class LeasePrintJobsHandler(IPrintJobQueue queue, CurrentAgent currentAgent, TimeProvider clock)
{
    public const int DefaultMaxJobs = 5;
    public const int MaxJobsLimit = 20;

    public async Task<Result<LeasedPrintJobsResponse>> HandleAsync(LeasePrintJobsRequest request, CancellationToken cancellationToken)
    {
        if (request.MaxJobs is < 1 or > MaxJobsLimit)
        {
            var errors = new FieldErrors();
            errors.Add("maxJobs", $"Peça de 1 a {MaxJobsLimit} tarefas.");
            return errors.ToError();
        }

        var jobs = await queue.LeaseAsync(currentAgent.RequireAgentId(), request.MaxJobs ?? DefaultMaxJobs, clock.GetUtcNow(), cancellationToken);
        return new LeasedPrintJobsResponse(jobs.Select(LeasedPrintJob.From).ToList());
    }
}
