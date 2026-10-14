using Brasa.Application.Shared.Events;
using Brasa.Application.Shared.Persistence;
using Brasa.Domain.Ordering;
using Brasa.Domain.Payments;

namespace Brasa.Application.Payments;

public sealed class CreatePaymentHandler(IAppDbContext db, TimeProvider clock) : IDomainEventHandler<OrderPlaced>
{
    public Task HandleAsync(OrderPlaced domainEvent, CancellationToken cancellationToken)
    {
        var order = domainEvent.Order;
        db.Payments.Add(new Payment(order.Id, order.PaymentMethod, order.Total, clock.GetUtcNow()));
        return Task.CompletedTask;
    }
}
