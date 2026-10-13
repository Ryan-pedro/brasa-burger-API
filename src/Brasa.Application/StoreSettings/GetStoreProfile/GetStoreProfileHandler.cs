using Brasa.Application.Shared.Persistence;
using Brasa.Domain.StoreSettings;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.StoreSettings.GetStoreProfile;

public sealed class GetStoreProfileHandler(IAppDbContext db)
{
    /// <returns>O perfil da loja, ou nulo se a loja ainda não foi configurada.</returns>
    public async Task<StoreProfileResponse?> HandleAsync(CancellationToken cancellationToken)
    {
        return await db.Stores
            .AsNoTracking()
            .Where(store => store.Id == Store.SingletonId)
            .Select(store => new StoreProfileResponse(store.Name, store.TimeZoneId))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
