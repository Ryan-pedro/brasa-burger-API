using Brasa.Domain.Shared;

namespace Brasa.Domain.Printing;

public sealed record PrintAgentPresenceChanged(PrintAgent Agent) : IDomainEvent;
