using Brasa.Application.Shared.Persistence;
using Brasa.Domain.Ordering;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Ordering.Management;

internal static class OrderDetailQuery
{
    public static async Task<OrderDetailResponse> DetailAsync(this IAppDbContext db, Order order, CancellationToken cancellationToken)
    {
        var payment = await db.Payments.AsNoTracking().FirstOrDefaultAsync(p => p.OrderId == order.Id, cancellationToken);
        var ticket = await db.KitchenTickets.AsNoTracking().FirstOrDefaultAsync(t => t.OrderId == order.Id, cancellationToken);

        return OrderDetailResponse.From(order, payment, ticket);
    }
}
