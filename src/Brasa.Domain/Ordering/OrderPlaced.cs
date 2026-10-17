using Brasa.Domain.Shared;

namespace Brasa.Domain.Ordering;

public sealed record OrderPlaced(Order Order) : IDomainEvent;
