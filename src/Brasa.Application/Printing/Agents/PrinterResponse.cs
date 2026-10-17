using Brasa.Domain.Printing;

namespace Brasa.Application.Printing.Agents;

public sealed record PrinterResponse(
    Guid Id,
    Guid AgentId,
    string Name,
    string PaperWidth,
    string Address,
    bool IsActive,
    bool IsDefault,
    string Status,
    string? StatusMessage,
    DateTimeOffset? StatusReportedAt)
{
    public static PrinterResponse From(Printer printer) =>
        new(
            printer.Id,
            printer.AgentId,
            printer.Name,
            printer.PaperWidth.ToString(),
            printer.Address,
            printer.IsActive,
            printer.IsDefault,
            printer.Status.ToString(),
            printer.StatusMessage,
            printer.StatusReportedAt);
}
