using Brasa.Application.Ordering.Shared;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Ordering.Management;

public sealed class GetOrderHandler(IAppDbContext db)
{
    public async Task<Result<OrderDetailResponse>> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var order = await db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        return order is null ? OrderErrors.NotFound : await db.DetailAsync(order, cancellationToken);
    }
}
