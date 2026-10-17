namespace Brasa.Application.Printing.Agents;

public sealed record PrinterStatusReport(Guid? PrinterId, string? Status, string? Message);
