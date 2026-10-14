using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Catalog.OptionGroups;

public sealed class CreateOptionGroupHandler(IAppDbContext db)
{
    public async Task<Result<OptionGroupResponse>> HandleAsync(OptionGroupRequest request, CancellationToken cancellationToken)
    {
        var errors = new FieldErrors();
        var valid = OptionGroupRules.Validate(request, null, errors);
        if (valid is null)
        {
            return errors.ToError();
        }

        var group = new OptionGroup(valid.Name, valid.Kind, valid.MinSelections, valid.MaxSelections);
        group.SetOptions(valid.Options);

        db.OptionGroups.Add(group);
        await db.SaveChangesAsync(cancellationToken);

        return OptionGroupResponse.From(group, productCount: 0);
    }
}
