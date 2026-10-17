using Brasa.Application.Ordering.Shared;
using Brasa.Application.Shared.Persistence;
using Brasa.Domain.Ordering;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Privacy;

public sealed class PersonalDataRemoval(IAppDbContext db, TimeProvider clock)
{
    public async Task RemoveAsync(IReadOnlyList<Order> orders, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var ordersById = orders.ToDictionary(order => order.Id);
        var orderIds = ordersById.Keys.ToList();

        foreach (var order in orders)
        {
            order.RemovePersonalData(now);
        }

        var tickets = await db.KitchenTickets.Where(ticket => orderIds.Contains(ticket.OrderId)).ToListAsync(cancellationToken);
        foreach (var ticket in tickets)
        {
            ticket.RemovePersonalData(OrderLabels.Customer(ordersById[ticket.OrderId]));
        }

        await db.SaveChangesAsync(cancellationToken);

        if (tickets.Count == 0)
        {
            return;
        }

        var ticketIds = tickets.Select(ticket => ticket.Id).ToList();
        await db.PrintJobs.Where(job => job.TicketId != null && ticketIds.Contains(job.TicketId.Value)).ExecuteDeleteAsync(cancellationToken);
    }
}
