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
    DateTimeOffset UpdatedAt,
    string? PrintStatus)
{
    public static TicketResponse From(KitchenTicket ticket, string? printStatus) =>
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
            ticket.UpdatedAt,
            printStatus);
}
