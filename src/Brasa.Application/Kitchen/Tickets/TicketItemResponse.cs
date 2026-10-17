namespace Brasa.Application.Kitchen.Tickets;

public sealed record TicketItemResponse(int Quantity, string Name, IReadOnlyList<string> Modifiers, string? Notes);
