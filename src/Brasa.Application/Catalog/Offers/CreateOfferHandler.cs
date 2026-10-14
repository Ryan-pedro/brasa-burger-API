using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Catalog;

namespace Brasa.Application.Catalog.Offers;

public sealed class CreateOfferHandler(IAppDbContext db, TimeProvider clock)
{
    public async Task<Result<OfferResponse>> HandleAsync(OfferRequest request, CancellationToken cancellationToken)
    {
        var errors = new FieldErrors();
        var valid = OfferRules.Validate(request, null, clock.GetUtcNow(), errors);
        if (valid is not null)
        {
            await db.ValidateProductAsync(valid, errors, cancellationToken);
        }

        if (valid is null || errors.Any)
        {
            return errors.ToError();
        }

        var offer = valid.Kind == OfferKind.ProductPrice
            ? Offer.ForProduct(valid.Label, valid.ProductId!.Value, valid.PromoPrice!.Value, valid.StartsAt, valid.EndsAt)
            : Offer.ForOrderAmount(valid.Label, valid.MinimumSubtotal!.Value, valid.Discount!.Value, valid.StartsAt, valid.EndsAt);

        if (request.IsActive == false)
        {
            offer.Deactivate();
        }

        db.Offers.Add(offer);
        await db.SaveChangesAsync(cancellationToken);

        return OfferResponse.From(offer);
    }
}
