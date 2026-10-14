using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Catalog.Offers;

internal static class OfferProductRules
{
    public static async Task ValidateProductAsync(this IAppDbContext db, ValidOffer offer, FieldErrors errors, CancellationToken cancellationToken)
    {
        if (offer.Kind != OfferKind.ProductPrice)
        {
            return;
        }

        var price = await db.Products
            .Where(product => product.Id == offer.ProductId)
            .Select(product => (decimal?)product.Price)
            .FirstOrDefaultAsync(cancellationToken);

        if (price is null)
        {
            errors.Add("productId", "Produto não encontrado.");
        }
        else if (offer.PromoPrice >= price)
        {
            errors.Add("promoPriceInCents", "O preço promocional precisa ser menor que o preço do produto.");
        }
    }

    public static void ApplyTo(this ValidOffer valid, Offer offer)
    {
        offer.Relabel(valid.Label);
        offer.Reschedule(valid.StartsAt, valid.EndsAt);

        if (valid.Kind == OfferKind.ProductPrice)
        {
            offer.SetProductTerms(valid.ProductId!.Value, valid.PromoPrice!.Value);
        }
        else
        {
            offer.SetOrderTerms(valid.MinimumSubtotal!.Value, valid.Discount!.Value);
        }
    }
}
