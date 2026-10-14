using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Catalog.OptionGroups;

public sealed class SetOptionAvailabilityHandler(IAppDbContext db, AuditTrail audit)
{
    public async Task<Result<OptionResponse>> HandleAsync(Guid groupId, Guid optionId, OptionAvailabilityRequest request, CancellationToken cancellationToken)
    {
        if (request.IsAvailable is not { } isAvailable)
        {
            var errors = new FieldErrors();
            errors.Add("isAvailable", "Informe se a opção está disponível.");
            return errors.ToError();
        }

        var group = await db.OptionGroups.FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);
        if (group is null)
        {
            return OptionGroupErrors.NotFound;
        }

        var option = group.FindOption(optionId);
        if (option is null)
        {
            return OptionGroupErrors.OptionNotFound;
        }

        group.SetOptionAvailability(optionId, isAvailable);
        audit.Record("option.availability_changed", optionId, new { isAvailable });
        await db.SaveChangesAsync(cancellationToken);

        return OptionResponse.From(option);
    }
}
