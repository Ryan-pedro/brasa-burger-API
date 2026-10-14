using Brasa.Domain.Catalog;
using Brasa.Domain.Ordering;

namespace Brasa.Application.Ordering.Shared;

public static class OptionText
{
    public static string ForCustomer(OrderItemOption option) => option.Kind switch
    {
        OptionKind.Choice => $"{option.GroupName}: {option.OptionName}",
        OptionKind.Removal => $"Sem {option.OptionName.ToLowerInvariant()}",
        _ => $"+ {option.OptionName}",
    };

    public static string ForKitchen(OrderItemOption option) => option.Kind switch
    {
        OptionKind.Choice => $"{option.GroupName}: {option.OptionName}".ToUpperInvariant(),
        OptionKind.Removal => $"SEM {option.OptionName.ToUpperInvariant()}",
        _ => $"+ {option.OptionName.ToUpperInvariant()}",
    };
}
