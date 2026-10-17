using Brasa.Domain.Ordering;

namespace Brasa.Domain.StoreSettings;

/// <summary>
/// Os dados da loja. O sistema atende uma única loja, então existe um único registro.
/// </summary>
public sealed class Store
{
    /// <summary>Identificador fixo do único registro da loja.</summary>
    public static readonly Guid SingletonId = new("00000000-0000-0000-0000-000000000001");

    public const int NameMaxLength = 120;

    public const decimal MaxDeliveryFee = 999.99m;

    public const decimal MaxMinimumOrder = 9999.99m;

    public const int MaxAcceptanceTimeoutMinutes = 120;

    public const int DefaultAcceptanceTimeoutMinutes = 10;

    // Usado pelo EF Core ao materializar a entidade.
    private Store()
    {
    }

    public Store(string name, string timeZoneId)
    {
        Id = SingletonId;
        Rename(name);
        ChangeTimeZone(timeZoneId);
        IsOpen = true;
        AcceptsDineIn = true;
        AcceptsPickup = true;
        AcceptsDelivery = true;
        AcceptanceTimeoutMinutes = DefaultAcceptanceTimeoutMinutes;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    /// <summary>Fuso horário da loja no padrão IANA (ex.: "America/Sao_Paulo").</summary>
    public string TimeZoneId { get; private set; } = null!;

    public bool IsOpen { get; private set; }

    public bool AcceptsDineIn { get; private set; }

    public bool AcceptsPickup { get; private set; }

    public bool AcceptsDelivery { get; private set; }

    public decimal DeliveryFee { get; private set; }

    public decimal MinimumOrder { get; private set; }

    public bool ManualAcceptance { get; private set; }

    public int AcceptanceTimeoutMinutes { get; private set; }

    public bool AcceptsOnlinePix { get; private set; }

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var trimmed = name.Trim();
        if (trimmed.Length > NameMaxLength)
        {
            throw new ArgumentException($"O nome da loja pode ter no máximo {NameMaxLength} caracteres.", nameof(name));
        }

        Name = trimmed;
    }

    public void ChangeTimeZone(string timeZoneId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZoneId);
        TimeZoneId = timeZoneId;
    }

    public void Open() => IsOpen = true;

    public void Close() => IsOpen = false;

    public void SetOrderTypes(bool dineIn, bool pickup, bool delivery)
    {
        if (!dineIn && !pickup && !delivery)
        {
            throw new ArgumentException("A loja precisa aceitar ao menos um tipo de pedido.");
        }

        AcceptsDineIn = dineIn;
        AcceptsPickup = pickup;
        AcceptsDelivery = delivery;
    }

    public bool Accepts(OrderType type) => type switch
    {
        OrderType.DineIn => AcceptsDineIn,
        OrderType.Pickup => AcceptsPickup,
        OrderType.Delivery => AcceptsDelivery,
        _ => false,
    };

    public decimal DeliveryFeeFor(OrderType type) => type == OrderType.Delivery ? DeliveryFee : 0m;

    public void SetDeliveryFee(decimal fee)
    {
        RequireAmount(fee, MaxDeliveryFee, nameof(fee));
        DeliveryFee = fee;
    }

    public void SetMinimumOrder(decimal minimum)
    {
        RequireAmount(minimum, MaxMinimumOrder, nameof(minimum));
        MinimumOrder = minimum;
    }

    public void SetAcceptance(bool manual, int timeoutMinutes)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(timeoutMinutes, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(timeoutMinutes, MaxAcceptanceTimeoutMinutes);

        ManualAcceptance = manual;
        AcceptanceTimeoutMinutes = timeoutMinutes;
    }

    public void SetOnlinePix(bool accepts) => AcceptsOnlinePix = accepts;

    private static void RequireAmount(decimal amount, decimal max, string parameterName)
    {
        if (amount < 0 || amount > max || decimal.Round(amount, 2) != amount)
        {
            throw new ArgumentOutOfRangeException(parameterName, amount, $"O valor vai de zero a {max}, com no máximo duas casas.");
        }
    }
}
