using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Application.StoreSettings.Shared;

namespace Brasa.Application.StoreSettings.Settings;

public sealed class GetStoreSettingsHandler(IAppDbContext db)
{
    public async Task<Result<StoreSettingsResponse>> HandleAsync(CancellationToken cancellationToken)
    {
        var store = await db.CurrentStoreAsync(cancellationToken);
        return store is null ? StoreErrors.NotConfigured : StoreSettingsResponse.From(store);
    }
}
