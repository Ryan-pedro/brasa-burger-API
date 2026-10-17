using Brasa.Domain.Shared;

namespace Brasa.Domain.Kitchen;

public sealed record TicketIssued(KitchenTicket Ticket) : IDomainEvent;
