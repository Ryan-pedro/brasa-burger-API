using Brasa.Domain.Catalog;

namespace Brasa.Application.Catalog.Offers;

internal sealed record ValidOffer(
    string Label,
    OfferKind Kind,
    Guid? ProductId,
    decimal? PromoPrice,
    decimal? MinimumSubtotal,
    decimal? Discount,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt);
