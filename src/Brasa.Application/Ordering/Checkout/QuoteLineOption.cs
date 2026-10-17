namespace Brasa.Application.Ordering.Checkout;

public sealed record QuoteLineOption(Guid Id, string GroupName, string Name, string Kind, long PriceDeltaInCents);
