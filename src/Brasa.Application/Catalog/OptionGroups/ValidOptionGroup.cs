using Brasa.Domain.Catalog;

namespace Brasa.Application.Catalog.OptionGroups;

internal sealed record ValidOptionGroup(string Name, OptionKind Kind, int MinSelections, int MaxSelections, IReadOnlyList<OptionDraft> Options);
