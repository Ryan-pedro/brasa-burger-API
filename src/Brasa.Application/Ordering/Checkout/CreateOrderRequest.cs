namespace Brasa.Application.Ordering.Checkout;

public sealed record CreateOrderRequest(
    string? Type,
    IReadOnlyList<CartItemRequest>? Items,
    CustomerRequest? Customer,
    AddressRequest? Address,
    string? Notes,
    string? PaymentMethod,
    long? ChangeForInCents,
    long? ExpectedTotalInCents);
