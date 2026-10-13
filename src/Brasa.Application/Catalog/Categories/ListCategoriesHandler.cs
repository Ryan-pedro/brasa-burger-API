using Brasa.Application.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Catalog.Categories;

public sealed class ListCategoriesHandler(IAppDbContext db)
{
    public async Task<IReadOnlyList<CategoryResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        var categories = await db.Categories
            .AsNoTracking()
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .ToListAsync(cancellationToken);

        var counts = await db.Products
            .GroupBy(product => product.CategoryId)
            .Select(group => new { CategoryId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.CategoryId, row => row.Count, cancellationToken);

        return categories
            .Select(category => CategoryResponse.From(category, counts.GetValueOrDefault(category.Id)))
            .ToList();
    }
}
