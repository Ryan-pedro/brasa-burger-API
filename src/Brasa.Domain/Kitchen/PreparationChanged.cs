using Brasa.Domain.Shared;

namespace Brasa.Domain.Kitchen;

public sealed record PreparationChanged(KitchenTicket Ticket) : IDomainEvent;
