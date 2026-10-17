namespace Brasa.Application.Catalog.Categories;

/// <summary>Corpo de criação e de edição. Campos nulos na edição mantêm o valor atual.</summary>
public sealed record CategoryRequest(string? Name, int? SortOrder, bool? IsActive);
