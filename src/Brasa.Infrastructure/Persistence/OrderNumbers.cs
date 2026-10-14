using Brasa.Application.Ordering.Checkout;
using Brasa.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Infrastructure.Persistence;

internal sealed class OrderNumbers(AppDbContext db) : IOrderNumbers
{
    private const string NextNumberSql =
        "SELECT nextval('ordering." + OrderConfiguration.NumberSequence + "')::int AS \"Value\"";

    public Task<int> NextAsync(CancellationToken cancellationToken) =>
        db.Database.SqlQueryRaw<int>(NextNumberSql).SingleAsync(cancellationToken);
}
