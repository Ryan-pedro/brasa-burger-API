using Brasa.Application.Kitchen.Tickets;

namespace Brasa.Application.Kitchen.Board;

public sealed record BoardResponse(DateTimeOffset ServerTime, IReadOnlyList<TicketResponse> Tickets, IReadOnlyList<BoardPrinter> Printers);
