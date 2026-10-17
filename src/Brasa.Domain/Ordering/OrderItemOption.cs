using Brasa.Domain.Catalog;

namespace Brasa.Domain.Ordering;

public sealed class OrderItemOption
{
    private OrderItemOption()
    {
    }

    internal OrderItemOption(PricedOption option)
    {
        GroupName = option.GroupName;
        OptionName = option.OptionName;
        Kind = option.Kind;
        PriceDelta = option.PriceDelta;
    }

    public string GroupName { get; private set; } = null!;

    public string OptionName { get; private set; } = null!;

    public OptionKind Kind { get; private set; }

    public decimal PriceDelta { get; private set; }
}
