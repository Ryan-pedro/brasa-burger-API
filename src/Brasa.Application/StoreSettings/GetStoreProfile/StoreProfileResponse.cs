namespace Brasa.Application.StoreSettings.GetStoreProfile;

/// <summary>Perfil público da loja, carregado pelo Storefront ao iniciar.</summary>
public sealed record StoreProfileResponse(
    string Name,
    string TimeZoneId,
    bool IsOpen,
    IReadOnlyList<string> OrderTypes,
    long DeliveryFeeInCents,
    long MinimumOrderInCents);
