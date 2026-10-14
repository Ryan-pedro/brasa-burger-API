using Brasa.Application.Shared;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.StoreSettings.Shared;

namespace Brasa.Application.StoreSettings.GetStoreProfile;

public sealed class GetStoreProfileHandler(IAppDbContext db)
{
    /// <returns>O perfil da loja, ou nulo se a loja ainda não foi configurada.</returns>
    public async Task<StoreProfileResponse?> HandleAsync(CancellationToken cancellationToken)
    {
        var store = await db.CurrentStoreAsync(cancellationToken);

        return store is null
            ? null
            : new StoreProfileResponse(
                store.Name,
                store.TimeZoneId,
                store.IsOpen,
                store.OrderTypes(),
                Money.ToCents(store.DeliveryFee),
                Money.ToCents(store.MinimumOrder));
    }
}
