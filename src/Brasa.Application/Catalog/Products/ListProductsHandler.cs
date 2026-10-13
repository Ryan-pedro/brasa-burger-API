using Brasa.Application.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Catalog.Products;

public sealed class ListProductsHandler(IAppDbContext db)
{
    /// <param name="categoryId">Opcional: só os produtos dessa categoria.</param>
    public async Task<IReadOnlyList<ProductResponse>> HandleAsync(Guid? categoryId, CancellationToken cancellationToken)
    {
        var query = db.Products.AsNoTracking();
        if (categoryId is { } id)
        {
            query = query.Where(product => product.CategoryId == id);
        }

        var products = await query
            .OrderBy(product => product.CategoryId)
            .ThenBy(product => product.SortOrder)
            .ThenBy(product => product.Name)
            .ToListAsync(cancellationToken);

        return products.Select(ProductResponse.From).ToList();
    }
}
