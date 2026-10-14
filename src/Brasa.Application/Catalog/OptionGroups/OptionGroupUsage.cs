using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Catalog.OptionGroups;

internal static class OptionGroupUsage
{
    public static Task<Dictionary<Guid, int>> ProductCountsByOptionGroupAsync(this IAppDbContext db, CancellationToken cancellationToken) =>
        db.Products
            .SelectMany(product => product.OptionGroups)
            .GroupBy(link => link.OptionGroupId)
            .Select(group => new { Id = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.Id, row => row.Count, cancellationToken);
}
