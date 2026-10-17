using Brasa.Application.Shared.Results;

namespace Brasa.Application.Catalog.Offers;

public static class OfferErrors
{
    public static readonly Error NotFound = Error.NotFound("offer_not_found", "Oferta não encontrada.");
}
