using Brasa.Application.Shared.Events;
using Brasa.Application.Shared.Persistence;
using Brasa.Domain.Kitchen;
using Brasa.Domain.Ordering;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Ordering.Reactions;

public sealed class FollowPreparationHandler(IAppDbContext db, TimeProvider clock) : IDomainEventHandler<PreparationChanged>
{
    public async Task HandleAsync(PreparationChanged domainEvent, CancellationToken cancellationToken)
    {
        var order = await db.Orders.FirstAsync(o => o.Id == domainEvent.Ticket.OrderId, cancellationToken);
        var now = clock.GetUtcNow();

        switch (domainEvent.Ticket.Preparation)
        {
            case PreparationStatus.Queued:
                order.FollowPreparation(OrderStatus.Confirmed, now);
                break;
            case PreparationStatus.InPreparation:
                order.FollowPreparation(OrderStatus.InPreparation, now);
                break;
            case PreparationStatus.Ready:
                order.FollowPreparation(OrderStatus.Ready, now);
                break;
            case PreparationStatus.Dispatched:
                order.Dispatch(now);
                break;
        }
    }
}
