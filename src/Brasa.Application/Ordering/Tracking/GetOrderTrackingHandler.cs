using Brasa.Application.Ordering.Shared;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Ordering;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Ordering.Tracking;

public sealed class GetOrderTrackingHandler(IAppDbContext db)
{
    public async Task<Result<OrderTrackingResponse>> HandleAsync(string token, CancellationToken cancellationToken)
    {
        if (token.Length != Order.TrackingTokenLength)
        {
            return OrderErrors.NotFound;
        }

        var order = await db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.TrackingToken == token, cancellationToken);
        return order is null ? OrderErrors.NotFound : OrderTrackingResponse.From(order);
    }
}
