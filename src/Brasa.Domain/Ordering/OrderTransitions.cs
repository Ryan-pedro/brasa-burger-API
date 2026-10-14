namespace Brasa.Domain.Ordering;

public static class OrderTransitions
{
    private static readonly Dictionary<OrderStatus, OrderStatus[]> Allowed = new()
    {
        [OrderStatus.Placed] = [OrderStatus.Confirmed, OrderStatus.Rejected],
        [OrderStatus.Confirmed] = [OrderStatus.InPreparation, OrderStatus.Cancelled],
        [OrderStatus.InPreparation] = [OrderStatus.Ready, OrderStatus.Confirmed, OrderStatus.Cancelled],
        [OrderStatus.Ready] = [OrderStatus.OutForDelivery, OrderStatus.Completed, OrderStatus.InPreparation],
        [OrderStatus.OutForDelivery] = [OrderStatus.Completed],
    };

    public static bool IsAllowed(OrderStatus from, OrderStatus to) =>
        Allowed.TryGetValue(from, out var targets) && targets.Contains(to);

    public static bool IsFinal(OrderStatus status) => !Allowed.ContainsKey(status);
}
