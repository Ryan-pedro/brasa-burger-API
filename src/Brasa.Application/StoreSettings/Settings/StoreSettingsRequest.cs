namespace Brasa.Application.StoreSettings.Settings;

public sealed record StoreSettingsRequest(
    string? Name,
    bool? AcceptsDineIn,
    bool? AcceptsPickup,
    bool? AcceptsDelivery,
    long? DeliveryFeeInCents,
    long? MinimumOrderInCents,
    bool? ManualAcceptance,
    int? AcceptanceTimeoutMinutes,
    bool? AcceptsOnlinePix);
