using Brasa.Domain.Ordering;
using Brasa.Domain.Payments;

namespace Brasa.Application.Ordering.Management;

public static class OrderActions
{
    public const string ReceivePayment = "receivePayment";

    public static OrderStatus Target(OrderAction action) => action switch
    {
        OrderAction.Accept => OrderStatus.Confirmed,
        OrderAction.Reject => OrderStatus.Rejected,
        OrderAction.Cancel => OrderStatus.Cancelled,
        _ => OrderStatus.Completed,
    };

    public static IReadOnlyList<string> Available(Order order, Payment? payment)
    {
        var actions = Enum.GetValues<OrderAction>()
            .Where(action => order.CanMoveTo(Target(action)) && (action != OrderAction.Accept || order.Status == OrderStatus.Placed))
            .Select(Name)
            .ToList();

        if (payment?.Status == PaymentStatus.Pending && order.Status is not (OrderStatus.Cancelled or OrderStatus.Rejected))
        {
            actions.Add(ReceivePayment);
        }

        return actions;
    }

    public static string Name(OrderAction action)
    {
        var text = action.ToString();
        return char.ToLowerInvariant(text[0]) + text[1..];
    }
}
