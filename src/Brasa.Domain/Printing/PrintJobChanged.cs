using Brasa.Domain.Shared;

namespace Brasa.Domain.Printing;

public sealed record PrintJobChanged(PrintJob Job) : IDomainEvent;
