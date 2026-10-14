using Brasa.Domain.Ordering;

namespace Brasa.Application.Ordering.Shared;

public static class OrderLabels
{
    public static string Customer(Order order) =>
        order.Type == OrderType.DineIn ? $"Mesa {order.Customer.Table}" : order.Customer.Name!;
}
