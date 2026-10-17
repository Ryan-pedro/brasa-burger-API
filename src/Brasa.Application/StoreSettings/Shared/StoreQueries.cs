using Brasa.Application.Shared.Persistence;
using Brasa.Domain.Ordering;
using Brasa.Domain.StoreSettings;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.StoreSettings.Shared;

public static class StoreQueries
{
    public static Task<Store?> CurrentStoreAsync(this IAppDbContext db, CancellationToken cancellationToken) =>
        db.Stores.FirstOrDefaultAsync(store => store.Id == Store.SingletonId, cancellationToken);

    public static IReadOnlyList<string> OrderTypes(this Store store) =>
        Enum.GetValues<OrderType>().Where(store.Accepts).Select(type => type.ToString()).ToList();
}
