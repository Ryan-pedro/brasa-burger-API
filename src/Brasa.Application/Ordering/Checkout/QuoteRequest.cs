namespace Brasa.Application.Ordering.Checkout;

public sealed record QuoteRequest(string? Type, IReadOnlyList<CartItemRequest>? Items);
