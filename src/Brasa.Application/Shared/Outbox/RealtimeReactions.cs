using Brasa.Application.Shared.Events;
using Brasa.Application.Shared.Persistence;
using Brasa.Domain.Kitchen;
using Brasa.Domain.Ordering;

namespace Brasa.Application.Shared.Outbox;

public sealed class RealtimeReactions(IAppDbContext db, TimeProvider clock)
    : IDomainEventHandler<OrderPlaced>, IDomainEventHandler<OrderStatusChanged>, IDomainEventHandler<TicketChanged>
{
    public Task HandleAsync(OrderPlaced domainEvent, CancellationToken cancellationToken) =>
        QueueOrder(domainEvent.Order);

    public Task HandleAsync(OrderStatusChanged domainEvent, CancellationToken cancellationToken) =>
        QueueOrder(domainEvent.Order);

    public Task HandleAsync(TicketChanged domainEvent, CancellationToken cancellationToken)
    {
        db.Queue(RealtimeChannels.Kitchen, RealtimeTypes.TicketChanged, domainEvent.Ticket.Id, clock.GetUtcNow());
        return Task.CompletedTask;
    }

    private Task QueueOrder(Order order)
    {
        db.Queue(RealtimeChannels.Ops, RealtimeTypes.OrderChanged, order.Id, clock.GetUtcNow());
        return Task.CompletedTask;
    }
}
