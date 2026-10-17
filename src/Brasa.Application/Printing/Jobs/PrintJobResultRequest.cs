namespace Brasa.Application.Printing.Jobs;

public sealed record PrintJobResultRequest(Guid? AttemptId, string? Outcome, string? Error);
