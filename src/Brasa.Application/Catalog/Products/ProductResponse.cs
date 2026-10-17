using Brasa.Application.Shared;
using Brasa.Domain.Catalog;

namespace Brasa.Application.Catalog.Products;

public sealed record ProductResponse(
    Guid Id,
    Guid CategoryId,
    string Name,
    string Description,
    long PriceInCents,
    string? Size,
    string? ImageUrl,
    bool IsAvailable,
    bool IsFeatured,
    int SortOrder,
    IReadOnlyList<Guid> OptionGroupIds)
{
    public static ProductResponse From(Product product) =>
        new(
            product.Id,
            product.CategoryId,
            product.Name,
            product.Description,
            Money.ToCents(product.Price),
            product.Size,
            product.ImageUrl,
            product.IsAvailable,
            product.IsFeatured,
            product.SortOrder,
            product.OptionGroupIds);
}
