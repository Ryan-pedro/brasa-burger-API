using Brasa.Application.Shared;
using Brasa.Application.Shared.Results;
using Brasa.Domain.StoreSettings;

namespace Brasa.Application.StoreSettings.Settings;

public sealed record StoreSettingsRequest(
    string? Name,
    bool? AcceptsDineIn,
    bool? AcceptsPickup,
    bool? AcceptsDelivery,
    long? DeliveryFeeInCents,
    long? MinimumOrderInCents,
    bool? ManualAcceptance,
    int? AcceptanceTimeoutMinutes);

public sealed record StoreStatusRequest(bool? IsOpen);

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
    int AcceptanceTimeoutMinutes)
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
            store.AcceptanceTimeoutMinutes);
}

internal static class StoreSettingsRules
{
    public static void Validate(StoreSettingsRequest request, Store store, FieldErrors errors)
    {
        var name = request.Name?.Trim();
        if (request.Name is not null && (string.IsNullOrEmpty(name) || name.Length > Store.NameMaxLength))
        {
            errors.Add("name", $"Informe o nome da loja, com até {Store.NameMaxLength} caracteres.");
        }

        var dineIn = request.AcceptsDineIn ?? store.AcceptsDineIn;
        var pickup = request.AcceptsPickup ?? store.AcceptsPickup;
        var delivery = request.AcceptsDelivery ?? store.AcceptsDelivery;
        if (!dineIn && !pickup && !delivery)
        {
            errors.Add("acceptsPickup", "Aceite ao menos um tipo de pedido.");
        }

        if (request.DeliveryFeeInCents is < 0 or > (long)(Store.MaxDeliveryFee * 100))
        {
            errors.Add("deliveryFeeInCents", "A taxa de entrega vai de R$ 0,00 a R$ 999,99.");
        }

        if (request.MinimumOrderInCents is < 0 or > (long)(Store.MaxMinimumOrder * 100))
        {
            errors.Add("minimumOrderInCents", "O pedido mínimo vai de R$ 0,00 a R$ 9.999,99.");
        }

        if (request.AcceptanceTimeoutMinutes is < 1 or > Store.MaxAcceptanceTimeoutMinutes)
        {
            errors.Add("acceptanceTimeoutMinutes", $"O prazo de aceite vai de 1 a {Store.MaxAcceptanceTimeoutMinutes} minutos.");
        }
    }
}
