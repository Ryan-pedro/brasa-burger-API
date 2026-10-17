namespace Brasa.Application.Catalog.OptionGroups;

public sealed record OptionGroupRequest(string? Name, string? Kind, int? MinSelections, int? MaxSelections, IReadOnlyList<OptionRequest>? Options);
