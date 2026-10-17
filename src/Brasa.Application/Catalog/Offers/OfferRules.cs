using Brasa.Application.Shared;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Catalog;

namespace Brasa.Application.Catalog.Offers;

internal static class OfferRules
{
    private const long MaxAmountInCents = 999_999;

    public static ValidOffer? Validate(OfferRequest request, Offer? existing, DateTimeOffset now, FieldErrors errors)
    {
        var label = request.Label?.Trim();
        if (string.IsNullOrEmpty(label))
        {
            errors.Add("label", "Informe o nome da oferta.");
        }
        else if (label.Length > Offer.LabelMaxLength)
        {
            errors.Add("label", $"Use no máximo {Offer.LabelMaxLength} caracteres.");
        }

        var kind = existing?.Kind ?? OfferKind.ProductPrice;
        if (request.Kind is not null || existing is null)
        {
            if (!EnumText.TryParse(request.Kind, out kind))
            {
                errors.Add("kind", $"Escolha o tipo: {EnumText.Names<OfferKind>()}.");
            }
            else if (existing is not null && existing.Kind != kind)
            {
                errors.Add("kind", "O tipo da oferta não pode mudar.");
            }
        }

        if (kind == OfferKind.ProductPrice)
        {
            if (request.ProductId is null || request.ProductId == Guid.Empty)
            {
                errors.Add("productId", "Escolha o produto.");
            }

            RequireAmount(request.PromoPriceInCents, "promoPriceInCents", errors);
        }
        else
        {
            RequireAmount(request.DiscountInCents, "discountInCents", errors);
            RequireAmount(request.MinimumSubtotalInCents, "minimumSubtotalInCents", errors);

            if (request.MinimumSubtotalInCents < request.DiscountInCents)
            {
                errors.Add("minimumSubtotalInCents", "O pedido mínimo precisa cobrir o desconto.");
            }
        }

        var startsAt = request.StartsAt ?? existing?.StartsAt ?? now;
        if (request.EndsAt <= startsAt)
        {
            errors.Add("endsAt", "O fim precisa ser depois do início.");
        }

        if (errors.Any)
        {
            return null;
        }

        return kind == OfferKind.ProductPrice
            ? new ValidOffer(label!, kind, request.ProductId, Money.FromCents(request.PromoPriceInCents!.Value), null, null, startsAt, request.EndsAt)
            : new ValidOffer(label!, kind, null, null, Money.FromCents(request.MinimumSubtotalInCents!.Value), Money.FromCents(request.DiscountInCents!.Value), startsAt, request.EndsAt);
    }

    private static void RequireAmount(long? cents, string field, FieldErrors errors)
    {
        if (cents is null or <= 0 or > MaxAmountInCents)
        {
            errors.Add(field, "Informe um valor entre R$ 0,01 e R$ 9.999,99.");
        }
    }
}
