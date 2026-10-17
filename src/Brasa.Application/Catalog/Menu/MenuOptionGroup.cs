namespace Brasa.Application.Catalog.Menu;

public sealed record MenuOptionGroup(Guid Id, string Name, string Kind, int MinSelections, int MaxSelections, IReadOnlyList<MenuOption> Options);
