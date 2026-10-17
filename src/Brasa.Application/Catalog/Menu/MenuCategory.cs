namespace Brasa.Application.Catalog.Menu;

public sealed record MenuCategory(Guid Id, string Name, string Slug, Guid? ParentId, IReadOnlyList<MenuProduct> Products);
