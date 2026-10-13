using Brasa.Application.Shared;
using Brasa.Application.Shared.Persistence;
using Brasa.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Catalog.Menu;

/// <summary>O cardápio público, numa resposta só: o Storefront monta todas as telas a partir dela.</summary>
public sealed record MenuResponse(IReadOnlyList<MenuCategory> Categories);

public sealed record MenuCategory(Guid Id, string Name, string Slug, IReadOnlyList<MenuProduct> Products);

public sealed record MenuProduct(
    Guid Id,
    string Name,
    string Description,
    long PriceInCents,
    string? Size,
    string? ImageUrl,
    bool IsAvailable,
    bool IsFeatured)
{
    public static MenuProduct From(Product product) =>
        new(
            product.Id,
            product.Name,
            product.Description,
            Money.ToCents(product.Price),
            product.Size,
            product.ImageUrl,
            product.IsAvailable,
            product.IsFeatured);
}

public sealed class GetMenuHandler(IAppDbContext db)
{
    /// <remarks>
    /// Só categorias ativas e com produtos. Produtos indisponíveis continuam na lista,
    /// marcados: o cliente vê que o item existe, mas não consegue pedir.
    /// </remarks>
    public async Task<MenuResponse> HandleAsync(CancellationToken cancellationToken)
    {
        var categories = await db.Categories
            .AsNoTracking()
            .Where(category => category.IsActive)
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .ToListAsync(cancellationToken);

        var categoryIds = categories.Select(category => category.Id).ToList();

        var products = await db.Products
            .AsNoTracking()
            .Where(product => categoryIds.Contains(product.CategoryId))
            .OrderBy(product => product.SortOrder)
            .ThenBy(product => product.Name)
            .ToListAsync(cancellationToken);

        var productsByCategory = products.ToLookup(product => product.CategoryId);

        var menu = categories
            .Where(category => productsByCategory[category.Id].Any())
            .Select(category => new MenuCategory(
                category.Id,
                category.Name,
                category.Slug,
                productsByCategory[category.Id].Select(MenuProduct.From).ToList()))
            .ToList();

        return new MenuResponse(menu);
    }
}
