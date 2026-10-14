using Brasa.Application.Shared.Results;
using Brasa.Domain.Kitchen;

namespace Brasa.Application.Kitchen.Tickets;

public sealed record TicketResponse(
    Guid Id,
    Guid OrderId,
    int OrderNumber,
    string OrderType,
    string CustomerLabel,
    string? Notes,
    string Preparation,
    string Document,
    IReadOnlyList<TicketItemResponse> Items,
    DateTimeOffset IssuedAt,
    DateTimeOffset UpdatedAt)
{
    public static TicketResponse From(KitchenTicket ticket) =>
        new(
            ticket.Id,
            ticket.OrderId,
            ticket.OrderNumber,
            ticket.OrderType.ToString(),
            ticket.CustomerLabel,
            ticket.Notes,
            ticket.Preparation.ToString(),
            ticket.Document.ToString(),
            ticket.Items.Select(item => new TicketItemResponse(item.Quantity, item.Name, item.Modifiers, item.Notes)).ToList(),
            ticket.IssuedAt,
            ticket.UpdatedAt);
}

public sealed record TicketItemResponse(int Quantity, string Name, IReadOnlyList<string> Modifiers, string? Notes);

public sealed record PreparationMoveRequest(string? From);

public static class TicketErrors
{
    public static readonly Error NotFound = Error.NotFound("ticket_not_found", "Comanda não encontrada.");

    public static readonly Error InvalidTransition = Error.Conflict("invalid_transition", "A comanda já mudou. Atualize o quadro.");
}
