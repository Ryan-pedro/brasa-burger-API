using Brasa.Domain.Shared;

namespace Brasa.Domain.Printing;

public sealed record PrinterStatusChanged(Printer Printer) : IDomainEvent;
