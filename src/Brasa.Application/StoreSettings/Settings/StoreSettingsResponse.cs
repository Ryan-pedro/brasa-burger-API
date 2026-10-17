using Brasa.Application.Shared;
using Brasa.Domain.StoreSettings;

namespace Brasa.Application.StoreSettings.Settings;

public sealed record StoreSettingsResponse(
    string Name,
    string TimeZoneId,
    bool IsOpen,
    bool AcceptsDineIn,
    bool AcceptsPickup,
    bool AcceptsDelivery,
    long DeliveryFeeInCents,
    long MinimumOrderInCents,
    bool ManualAcceptance,
    int AcceptanceTimeoutMinutes,
    bool AcceptsOnlinePix)
{
    public static StoreSettingsResponse From(Store store) =>
        new(
            store.Name,
            store.TimeZoneId,
            store.IsOpen,
            store.AcceptsDineIn,
            store.AcceptsPickup,
            store.AcceptsDelivery,
            Money.ToCents(store.DeliveryFee),
            Money.ToCents(store.MinimumOrder),
            store.ManualAcceptance,
            store.AcceptanceTimeoutMinutes,
            store.AcceptsOnlinePix);
}
