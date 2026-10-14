using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Application.StoreSettings.Shared;

namespace Brasa.Application.StoreSettings.Settings;

public sealed class SetStoreStatusHandler(IAppDbContext db, AuditTrail audit)
{
    public async Task<Result<StoreSettingsResponse>> HandleAsync(StoreStatusRequest request, CancellationToken cancellationToken)
    {
        if (request.IsOpen is not { } isOpen)
        {
            var errors = new FieldErrors();
            errors.Add("isOpen", "Informe se a loja está aberta.");
            return errors.ToError();
        }

        var store = await db.CurrentStoreAsync(cancellationToken);
        if (store is null)
        {
            return StoreErrors.NotConfigured;
        }

        if (isOpen)
        {
            store.Open();
        }
        else
        {
            store.Close();
        }

        audit.Record(isOpen ? "store.opened" : "store.closed", store.Id);
        await db.SaveChangesAsync(cancellationToken);

        return StoreSettingsResponse.From(store);
    }
}
