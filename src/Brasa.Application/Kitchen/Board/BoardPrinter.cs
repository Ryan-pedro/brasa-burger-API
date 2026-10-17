namespace Brasa.Application.Kitchen.Board;

public sealed record BoardPrinter(Guid Id, string Name, bool IsDefault, bool IsAgentOnline, string Status, string? StatusMessage);
