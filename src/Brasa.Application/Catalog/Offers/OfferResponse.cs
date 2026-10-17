using Brasa.Application.Shared;
using Brasa.Domain.Catalog;

namespace Brasa.Application.Catalog.Offers;

public sealed record OfferResponse(
    Guid Id,
    string Label,
    string Kind,
    Guid? ProductId,
    long? PromoPriceInCents,
    long? MinimumSubtotalInCents,
    long? DiscountInCents,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt,
    bool IsActive)
{
    public static OfferResponse From(Offer offer) =>
        new(
            offer.Id,
            offer.Label,
            offer.Kind.ToString(),
            offer.ProductId,
            Cents(offer.PromoPrice),
            Cents(offer.MinimumSubtotal),
            Cents(offer.DiscountAmount),
            offer.StartsAt,
            offer.EndsAt,
            offer.IsActive);

    private static long? Cents(decimal? amount) => amount is { } value ? Money.ToCents(value) : null;
}
