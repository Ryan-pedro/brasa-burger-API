using Brasa.Domain.Printing;

namespace Brasa.Application.Printing.Jobs;

public sealed record PrintJobResponse(
    Guid Id,
    Guid PrinterId,
    Guid? TicketId,
    string Reason,
    string Status,
    bool PossibleDuplicate,
    int AttemptCount,
    string? LastError,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt)
{
    public static PrintJobResponse From(PrintJob job) =>
        new(
            job.Id,
            job.PrinterId,
            job.TicketId,
            job.Reason.ToString(),
            job.Status.ToString(),
            job.PossibleDuplicate,
            job.Attempts.Count,
            job.LastError,
            job.CreatedAt,
            job.CompletedAt);
}
