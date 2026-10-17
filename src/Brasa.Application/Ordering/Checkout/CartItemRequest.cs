namespace Brasa.Application.Ordering.Checkout;

public sealed record CartItemRequest(Guid? ProductId, int? Quantity, IReadOnlyList<Guid>? OptionIds, string? Notes);
