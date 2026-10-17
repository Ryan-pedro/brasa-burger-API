using Brasa.Application.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Catalog.OptionGroups;

public sealed class ListOptionGroupsHandler(IAppDbContext db)
{
    public async Task<IReadOnlyList<OptionGroupResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        var groups = await db.OptionGroups
            .AsNoTracking()
            .OrderBy(group => group.Name)
            .ToListAsync(cancellationToken);

        var usage = await db.ProductCountsByOptionGroupAsync(cancellationToken);

        return groups.Select(group => OptionGroupResponse.From(group, usage.GetValueOrDefault(group.Id))).ToList();
    }
}
