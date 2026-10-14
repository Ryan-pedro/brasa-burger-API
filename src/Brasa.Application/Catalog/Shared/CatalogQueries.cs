using Brasa.Application.Shared.Persistence;
using Brasa.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Catalog.Shared;

public static class CatalogQueries
{
    public static async Task<IReadOnlyList<Category>> VisibleCategoriesAsync(this IAppDbContext db, CancellationToken cancellationToken)
    {
        var categories = await db.Categories
            .AsNoTracking()
            .Where(category => category.IsActive)
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .ToListAsync(cancellationToken);

        var activeIds = categories.Select(category => category.Id).ToHashSet();

        return categories
            .Where(category => category.ParentId is null || activeIds.Contains(category.ParentId.Value))
            .ToList();
    }

    public static async Task<IReadOnlyDictionary<Guid, OptionGroup>> OptionGroupsForAsync(
        this IAppDbContext db,
        IEnumerable<Product> products,
        CancellationToken cancellationToken)
    {
        var ids = products.SelectMany(product => product.OptionGroupIds).Distinct().ToList();

        return await db.OptionGroups
            .AsNoTracking()
            .Where(group => ids.Contains(group.Id))
            .ToDictionaryAsync(group => group.Id, cancellationToken);
    }

    public static async Task<IReadOnlyList<Offer>> ValidOffersAsync(this IAppDbContext db, DateTimeOffset moment, CancellationToken cancellationToken)
    {
        return await db.Offers
            .AsNoTracking()
            .Where(offer => offer.IsActive && offer.StartsAt <= moment && (offer.EndsAt == null || offer.EndsAt > moment))
            .OrderBy(offer => offer.Id)
            .ToListAsync(cancellationToken);
    }

    public static IReadOnlyList<OptionGroup> GroupsOf(this IReadOnlyDictionary<Guid, OptionGroup> groups, Product product) =>
        product.OptionGroupIds.Where(groups.ContainsKey).Select(id => groups[id]).ToList();

    public static bool IsOrderable(this Product product, IReadOnlyList<OptionGroup> groups) =>
        product.IsAvailable && groups.All(group => group.CanBeFulfilled);

    public static decimal? PromoPrice(this IReadOnlyList<Offer> offers, Product product) =>
        offers
            .Where(offer => offer.Kind == OfferKind.ProductPrice && offer.ProductId == product.Id && offer.PromoPrice < product.Price)
            .Min(offer => offer.PromoPrice);
}
