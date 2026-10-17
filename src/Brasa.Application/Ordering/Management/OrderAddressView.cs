namespace Brasa.Application.Ordering.Management;

public sealed record OrderAddressView(string Street, string Number, string? Complement, string District);
