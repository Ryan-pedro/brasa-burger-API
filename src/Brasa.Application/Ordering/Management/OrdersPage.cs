namespace Brasa.Application.Ordering.Management;

public sealed record OrdersPage(IReadOnlyList<OrderSummary> Items, string? NextCursor);
