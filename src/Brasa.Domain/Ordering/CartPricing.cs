using Brasa.Domain.Catalog;

namespace Brasa.Domain.Ordering;

public static class CartPricing
{
    public static PriceBreakdown Calculate(IReadOnlyList<CartLine> lines, IEnumerable<Offer> offers, decimal deliveryFee, DateTimeOffset moment)
    {
        var subtotal = lines.Sum(line => line.Total);

        var discount = offers
            .Where(offer => offer.IsValidAt(moment))
            .OrderBy(offer => offer.Id)
            .Select(offer => new AppliedDiscount(offer.Id, offer.Label, DiscountOf(offer, lines, subtotal)))
            .Where(candidate => candidate.Amount > 0)
            .MaxBy(candidate => candidate.Amount);

        return new PriceBreakdown(subtotal, discount, deliveryFee);
    }

    private static decimal DiscountOf(Offer offer, IReadOnlyList<CartLine> lines, decimal subtotal) => offer.Kind switch
    {
        OfferKind.ProductPrice => lines
            .Where(line => line.ProductId == offer.ProductId)
            .Sum(line => Math.Max(0m, line.BasePrice - offer.PromoPrice!.Value) * line.Quantity),
        OfferKind.OrderAmount => subtotal >= offer.MinimumSubtotal ? Math.Min(offer.DiscountAmount!.Value, subtotal) : 0m,
        _ => 0m,
    };
}
