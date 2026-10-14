using Brasa.Application.Ordering.Shared;
using Brasa.Application.Shared.Events;
using Brasa.Application.Shared.Persistence;
using Brasa.Domain.Kitchen;
using Brasa.Domain.Ordering;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Kitchen.Tickets;

public sealed class IssueTicketHandler(IAppDbContext db, TimeProvider clock) : IDomainEventHandler<OrderStatusChanged>
{
    public async Task HandleAsync(OrderStatusChanged domainEvent, CancellationToken cancellationToken)
    {
        var order = domainEvent.Order;
        var now = clock.GetUtcNow();

        if (order.Status == OrderStatus.Confirmed && domainEvent.Previous == OrderStatus.Placed)
        {
            db.KitchenTickets.Add(new KitchenTicket(
                order.Id,
                order.Number,
                order.Type,
                OrderLabels.Customer(order),
                order.Notes,
                order.Items.Select(item => new TicketItem(item.Quantity, item.ProductName, item.Options.Select(OptionText.ForKitchen).ToList(), item.Notes)).ToList(),
                now));
            return;
        }

        if (order.Status is not (OrderStatus.Cancelled or OrderStatus.Completed))
        {
            return;
        }

        var ticket = await db.KitchenTickets.FirstOrDefaultAsync(t => t.OrderId == order.Id, cancellationToken);
        if (ticket is null)
        {
            return;
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            ticket.Void(now);
        }
        else
        {
            ticket.Close(now);
        }
    }
}
