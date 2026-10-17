namespace Brasa.Application.Printing.Jobs;

public sealed record PrintJobsPage(IReadOnlyList<PrintJobResponse> Items, string? NextCursor);
