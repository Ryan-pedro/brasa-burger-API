using Brasa.Application.Shared;

namespace Brasa.Application.Ordering.Checkout;

public sealed record QuoteResponse(
    IReadOnlyList<QuoteLine> Lines,
    long SubtotalInCents,
    QuoteDiscount? Discount,
    long DeliveryFeeInCents,
    long TotalInCents,
    long MinimumOrderInCents)
{
    public static QuoteResponse From(PricedCart cart) =>
        new(
            cart.Lines.Select(QuoteLine.From).ToList(),
            Money.ToCents(cart.Price.Subtotal),
            cart.Price.Discount is { } discount ? new QuoteDiscount(discount.Label, Money.ToCents(discount.Amount)) : null,
            Money.ToCents(cart.Price.DeliveryFee),
            Money.ToCents(cart.Price.Total),
            Money.ToCents(cart.Store.MinimumOrder));
}
