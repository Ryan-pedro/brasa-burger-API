using Brasa.Application.Payments.Online;
using Brasa.Application.Shared;
using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Application.StoreSettings.Shared;

namespace Brasa.Application.StoreSettings.Settings;

public sealed class UpdateStoreSettingsHandler(IAppDbContext db, IPixGateway pixGateway, AuditTrail audit)
{
    public async Task<Result<StoreSettingsResponse>> HandleAsync(StoreSettingsRequest request, CancellationToken cancellationToken)
    {
        var store = await db.CurrentStoreAsync(cancellationToken);
        if (store is null)
        {
            return StoreErrors.NotConfigured;
        }

        var errors = new FieldErrors();
        StoreSettingsRules.Validate(request, store, pixGateway.IsConfigured, errors);
        if (errors.Any)
        {
            return errors.ToError();
        }

        if (request.Name is not null)
        {
            store.Rename(request.Name);
        }

        store.SetOrderTypes(
            request.AcceptsDineIn ?? store.AcceptsDineIn,
            request.AcceptsPickup ?? store.AcceptsPickup,
            request.AcceptsDelivery ?? store.AcceptsDelivery);

        if (request.DeliveryFeeInCents is { } fee)
        {
            store.SetDeliveryFee(Money.FromCents(fee));
        }

        if (request.MinimumOrderInCents is { } minimum)
        {
            store.SetMinimumOrder(Money.FromCents(minimum));
        }

        store.SetAcceptance(
            request.ManualAcceptance ?? store.ManualAcceptance,
            request.AcceptanceTimeoutMinutes ?? store.AcceptanceTimeoutMinutes);

        if (request.AcceptsOnlinePix is { } acceptsOnlinePix)
        {
            store.SetOnlinePix(acceptsOnlinePix);
        }

        audit.Record("store.settings_changed", store.Id, request);
        await db.SaveChangesAsync(cancellationToken);

        return StoreSettingsResponse.From(store);
    }
}
