using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Catalog.OptionGroups;

public sealed class UpdateOptionGroupHandler(IAppDbContext db)
{
    public async Task<Result<OptionGroupResponse>> HandleAsync(Guid id, OptionGroupRequest request, CancellationToken cancellationToken)
    {
        var group = await db.OptionGroups.FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
        if (group is null)
        {
            return OptionGroupErrors.NotFound;
        }

        var errors = new FieldErrors();
        var valid = OptionGroupRules.Validate(request, group, errors);
        if (valid is null)
        {
            return errors.ToError();
        }

        group.Rename(valid.Name);
        group.SetLimits(valid.MinSelections, valid.MaxSelections);
        group.SetOptions(valid.Options);

        await db.SaveChangesAsync(cancellationToken);

        var usage = await db.ProductCountsByOptionGroupAsync(cancellationToken);
        return OptionGroupResponse.From(group, usage.GetValueOrDefault(group.Id));
    }
}
