using Brasa.Application.Shared;
using Brasa.Domain.Ordering;

namespace Brasa.Application.Ordering.Management;

public sealed record OrderDetailItem(
    Guid ProductId,
    string Name,
    int Quantity,
    long UnitPriceInCents,
    long TotalInCents,
    IReadOnlyList<OrderDetailOption> Options,
    string? Notes)
{
    public static OrderDetailItem From(OrderItem item) =>
        new(
            item.ProductId,
            item.ProductName,
            item.Quantity,
            Money.ToCents(item.UnitPrice),
            Money.ToCents(item.Total),
            item.Options.Select(option => new OrderDetailOption(option.GroupName, option.OptionName, option.Kind.ToString(), Money.ToCents(option.PriceDelta))).ToList(),
            item.Notes);
}
