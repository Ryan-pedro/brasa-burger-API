using Brasa.Application.Ordering.Shared;
using Brasa.Application.Shared;
using Brasa.Domain.Ordering;

namespace Brasa.Application.Ordering.Checkout;

public sealed record CartItemRequest(Guid? ProductId, int? Quantity, IReadOnlyList<Guid>? OptionIds, string? Notes);

public sealed record QuoteRequest(string? Type, IReadOnlyList<CartItemRequest>? Items);

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

public sealed record QuoteLine(
    Guid ProductId,
    string Name,
    int Quantity,
    long UnitPriceInCents,
    long TotalInCents,
    IReadOnlyList<QuoteLineOption> Options,
    string? Notes)
{
    public static QuoteLine From(CartLine line) =>
        new(
            line.ProductId,
            line.ProductName,
            line.Quantity,
            Money.ToCents(line.UnitPrice),
            Money.ToCents(line.Total),
            line.Options.Select(option => new QuoteLineOption(option.OptionId, option.GroupName, option.OptionName, option.Kind.ToString(), Money.ToCents(option.PriceDelta))).ToList(),
            line.Notes);
}

public sealed record QuoteLineOption(Guid Id, string GroupName, string Name, string Kind, long PriceDeltaInCents);

public sealed record QuoteDiscount(string Label, long AmountInCents);
