namespace Brasa.Application.Ordering.Checkout;

public sealed record AddressRequest(string? Street, string? Number, string? Complement, string? District);
