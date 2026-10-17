using Brasa.Application.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Catalog.Products;

internal static class ProductOptionGroupRules
{
    public static async Task<bool> AllExistAsync(this IAppDbContext db, IReadOnlyList<Guid> optionGroupIds, CancellationToken cancellationToken) =>
        await db.OptionGroups.CountAsync(group => optionGroupIds.Contains(group.Id), cancellationToken) == optionGroupIds.Count;
}
