using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Catalog.Offers;

public sealed class UpdateOfferHandler(IAppDbContext db, TimeProvider clock)
{
    public async Task<Result<OfferResponse>> HandleAsync(Guid id, OfferRequest request, CancellationToken cancellationToken)
    {
        var offer = await db.Offers.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (offer is null)
        {
            return OfferErrors.NotFound;
        }

        var errors = new FieldErrors();
        var valid = OfferRules.Validate(request, offer, clock.GetUtcNow(), errors);
        if (valid is not null)
        {
            await db.ValidateProductAsync(valid, errors, cancellationToken);
        }

        if (valid is null || errors.Any)
        {
            return errors.ToError();
        }

        valid.ApplyTo(offer);

        if (request.IsActive is { } isActive)
        {
            if (isActive)
            {
                offer.Activate();
            }
            else
            {
                offer.Deactivate();
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        return OfferResponse.From(offer);
    }
}
