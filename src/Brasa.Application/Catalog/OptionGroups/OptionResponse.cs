using Brasa.Application.Shared;
using Brasa.Domain.Catalog;

namespace Brasa.Application.Catalog.OptionGroups;

public sealed record OptionResponse(Guid Id, string Name, long PriceDeltaInCents, bool IsAvailable)
{
    public static OptionResponse From(Option option) =>
        new(option.Id, option.Name, Money.ToCents(option.PriceDelta), option.IsAvailable);
}
