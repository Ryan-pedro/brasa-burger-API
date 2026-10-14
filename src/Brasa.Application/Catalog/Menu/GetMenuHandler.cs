using Brasa.Application.Catalog.Shared;
using Brasa.Application.Shared;
using Brasa.Application.Shared.Persistence;
using Brasa.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Catalog.Menu;

/// <summary>O cardápio público, numa resposta só: o Storefront monta todas as telas a partir dela.</summary>
public sealed record MenuResponse(IReadOnlyList<MenuCategory> Categories);

public sealed record MenuCategory(Guid Id, string Name, string Slug, Guid? ParentId, IReadOnlyList<MenuProduct> Products);

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

public sealed record MenuOptionGroup(Guid Id, string Name, string Kind, int MinSelections, int MaxSelections, IReadOnlyList<MenuOption> Options);

public sealed record MenuOption(Guid Id, string Name, long PriceDeltaInCents, bool IsAvailable);

public sealed class GetMenuHandler(IAppDbContext db, TimeProvider clock)
{
    public async Task<MenuResponse> HandleAsync(CancellationToken cancellationToken)
    {
        var categories = await db.VisibleCategoriesAsync(cancellationToken);
        var categoryIds = categories.Select(category => category.Id).ToList();

        var products = await db.Products
            .AsNoTracking()
            .Where(product => categoryIds.Contains(product.CategoryId))
            .OrderBy(product => product.SortOrder)
            .ThenBy(product => product.Name)
            .ToListAsync(cancellationToken);

        var groups = await db.OptionGroupsForAsync(products, cancellationToken);
        var offers = await db.ValidOffersAsync(clock.GetUtcNow(), cancellationToken);
        var productsByCategory = products.ToLookup(product => product.CategoryId);

        var menu = categories
            .Where(category => productsByCategory[category.Id].Any())
            .Select(category => new MenuCategory(
                category.Id,
                category.Name,
                category.Slug,
                category.ParentId,
                productsByCategory[category.Id].Select(product => ToMenuProduct(product, groups.GroupsOf(product), offers)).ToList()))
            .ToList();

        return new MenuResponse(menu);
    }

    private static MenuProduct ToMenuProduct(Product product, IReadOnlyList<OptionGroup> groups, IReadOnlyList<Offer> offers) =>
        new(
            product.Id,
            product.Name,
            product.Description,
            Money.ToCents(product.Price),
            product.Size,
            product.ImageUrl,
            product.IsOrderable(groups),
            product.IsFeatured,
            offers.PromoPrice(product) is { } promo ? Money.ToCents(promo) : null,
            groups.Select(ToMenuGroup).ToList());

    private static MenuOptionGroup ToMenuGroup(OptionGroup group) =>
        new(
            group.Id,
            group.Name,
            group.Kind.ToString(),
            group.MinSelections,
            group.MaxSelections,
            group.Options.Select(option => new MenuOption(option.Id, option.Name, Money.ToCents(option.PriceDelta), option.IsAvailable)).ToList());
}
