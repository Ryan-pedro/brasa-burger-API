namespace Brasa.Application.Printing.Jobs;

public sealed record LeasedPrintJobsResponse(IReadOnlyList<LeasedPrintJob> Jobs);
