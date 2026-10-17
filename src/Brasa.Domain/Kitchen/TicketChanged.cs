using Brasa.Domain.Shared;

namespace Brasa.Domain.Kitchen;

public sealed record TicketChanged(KitchenTicket Ticket) : IDomainEvent;
