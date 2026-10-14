using Brasa.Application.Catalog.Shared;
using Brasa.Application.Shared.Persistence;

namespace Brasa.Application.Catalog.Offers;

public sealed class ListActiveOffersHandler(IAppDbContext db, TimeProvider clock)
{
    public async Task<IReadOnlyList<OfferResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        var offers = await db.ValidOffersAsync(clock.GetUtcNow(), cancellationToken);
        return offers.Select(OfferResponse.From).ToList();
    }
}
