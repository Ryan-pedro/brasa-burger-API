using Brasa.Application.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Catalog.Offers;

public sealed class ListOffersHandler(IAppDbContext db)
{
    public async Task<IReadOnlyList<OfferResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        var offers = await db.Offers
            .AsNoTracking()
            .OrderByDescending(offer => offer.StartsAt)
            .ToListAsync(cancellationToken);

        return offers.Select(OfferResponse.From).ToList();
    }
}
