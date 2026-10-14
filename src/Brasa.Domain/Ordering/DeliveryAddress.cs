using Brasa.Domain.Shared;

namespace Brasa.Domain.Ordering;

public sealed record DeliveryAddress
{
    public const int StreetMaxLength = 120;
    public const int NumberMaxLength = 20;
    public const int ComplementMaxLength = 80;
    public const int DistrictMaxLength = 80;

    private DeliveryAddress()
    {
    }

    public DeliveryAddress(string street, string number, string? complement, string district)
    {
        Street = FreeText.Required(street, StreetMaxLength, nameof(street));
        Number = FreeText.Required(number, NumberMaxLength, nameof(number));
        Complement = FreeText.Clean(complement, ComplementMaxLength, nameof(complement));
        District = FreeText.Required(district, DistrictMaxLength, nameof(district));
    }

    public string Street { get; private init; } = null!;

    public string Number { get; private init; } = null!;

    public string? Complement { get; private init; }

    public string District { get; private init; } = null!;
}
