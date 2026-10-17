namespace Brasa.Application.Catalog.Offers;

public sealed record OfferRequest(
    string? Label,
    string? Kind,
    Guid? ProductId,
    long? PromoPriceInCents,
    long? MinimumSubtotalInCents,
    long? DiscountInCents,
    DateTimeOffset? StartsAt,
    DateTimeOffset? EndsAt,
    bool? IsActive);
