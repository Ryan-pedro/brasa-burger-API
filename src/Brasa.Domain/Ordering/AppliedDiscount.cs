namespace Brasa.Domain.Ordering;

public sealed record AppliedDiscount(Guid OfferId, string Label, decimal Amount);
