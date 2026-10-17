namespace Brasa.Application.Catalog.Menu;

public sealed record MenuProduct(
    Guid Id,
    string Name,
    string Description,
    long PriceInCents,
    string? Size,
    string? ImageUrl,
    bool IsAvailable,
    bool IsFeatured,
    long? PromoPriceInCents,
    IReadOnlyList<MenuOptionGroup> OptionGroups);
