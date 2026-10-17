namespace Brasa.Application.Printing.Printers;

public sealed record PrinterRequest(string? Name, string? PaperWidth, string? Address, bool? IsActive, bool? IsDefault);
