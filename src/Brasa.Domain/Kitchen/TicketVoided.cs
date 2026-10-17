using Brasa.Domain.Shared;

namespace Brasa.Domain.Kitchen;

public sealed record TicketVoided(KitchenTicket Ticket) : IDomainEvent;
