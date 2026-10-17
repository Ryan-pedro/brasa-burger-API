using Brasa.Domain.Catalog;

namespace Brasa.Application.Catalog.OptionGroups;

public sealed record OptionGroupResponse(
    Guid Id,
    string Name,
    string Kind,
    int MinSelections,
    int MaxSelections,
    IReadOnlyList<OptionResponse> Options,
    int ProductCount)
{
    public static OptionGroupResponse From(OptionGroup group, int productCount) =>
        new(
            group.Id,
            group.Name,
            group.Kind.ToString(),
            group.MinSelections,
            group.MaxSelections,
            group.Options.Select(OptionResponse.From).ToList(),
            productCount);
}
