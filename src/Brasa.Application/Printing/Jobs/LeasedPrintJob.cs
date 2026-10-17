using Brasa.Domain.Printing;

namespace Brasa.Application.Printing.Jobs;

public sealed record LeasedPrintJob(
    Guid JobId,
    Guid AttemptId,
    Guid PrinterId,
    string Reason,
    bool PossibleDuplicate,
    DateTimeOffset LeasedUntil,
    PrintDocument Document)
{
    public static LeasedPrintJob From(PrintJob job) =>
        new(job.Id, job.CurrentAttempt!.Id, job.PrinterId, job.Reason.ToString(), job.PossibleDuplicate, job.LeasedUntil!.Value, job.Document);
}
