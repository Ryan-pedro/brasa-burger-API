using Brasa.Application.Shared;
using Brasa.Domain.Ordering;

namespace Brasa.Application.Ordering.Checkout;

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
