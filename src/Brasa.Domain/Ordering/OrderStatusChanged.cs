using Brasa.Domain.Shared;

namespace Brasa.Domain.Ordering;

public sealed record OrderStatusChanged(Order Order, OrderStatus Previous) : IDomainEvent;
