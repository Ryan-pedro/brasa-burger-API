namespace Brasa.Application.Catalog.Products;

/// <summary>
/// Corpo de criação e de edição de produto. Preço em centavos (ADR-0012).
/// Na edição, <c>isAvailable</c>, <c>isFeatured</c> e <c>sortOrder</c> nulos mantêm o valor atual.
/// </summary>
public sealed record ProductRequest(
    Guid? CategoryId,
    string? Name,
    string? Description,
    long? PriceInCents,
    string? Size,
    string? ImageUrl,
    bool? IsAvailable,
    bool? IsFeatured,
    int? SortOrder,
    IReadOnlyList<Guid>? OptionGroupIds = null);
