using Brasa.Application.Shared;
using Brasa.Domain.Kitchen;
using Brasa.Domain.Ordering;
using Brasa.Domain.Payments;

namespace Brasa.Application.Ordering.Management;

public sealed record ReasonRequest(string? Reason);

public sealed record OrdersPage(IReadOnlyList<OrderSummary> Items, string? NextCursor);

public sealed record OrderSummary(
    Guid Id,
    int Number,
    string Status,
    string Type,
    string CustomerLabel,
    int ItemCount,
    long TotalInCents,
    string PaymentMethod,
    string? PaymentStatus,
    DateTimeOffset CreatedAt);

public sealed record OrderDetailResponse(
    Guid Id,
    int Number,
    string Status,
    string? StatusReason,
    string Type,
    OrderCustomerView Customer,
    OrderAddressView? Address,
    string? Notes,
    IReadOnlyList<OrderDetailItem> Items,
    long SubtotalInCents,
    string? DiscountLabel,
    long DiscountInCents,
    long DeliveryFeeInCents,
    long TotalInCents,
    OrderPaymentView Payment,
    OrderTicketView? Ticket,
    IReadOnlyList<string> Actions,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static OrderDetailResponse From(Order order, Payment? payment, KitchenTicket? ticket) =>
        new(
            order.Id,
            order.Number,
            order.Status.ToString(),
            order.StatusReason,
            order.Type.ToString(),
            new OrderCustomerView(order.Customer.Name, order.Customer.Phone, order.Customer.Table),
            order.DeliveryAddress is { } address
                ? new OrderAddressView(address.Street, address.Number, address.Complement, address.District)
                : null,
            order.Notes,
            order.Items.Select(OrderDetailItem.From).ToList(),
            Money.ToCents(order.Subtotal),
            order.DiscountLabel,
            Money.ToCents(order.DiscountAmount),
            Money.ToCents(order.DeliveryFee),
            Money.ToCents(order.Total),
            new OrderPaymentView(
                order.PaymentMethod.ToString(),
                payment?.Status.ToString(),
                Money.ToCents(order.Total),
                order.ChangeFor is { } change ? Money.ToCents(change) : null,
                payment?.PaidAt),
            ticket is null ? null : new OrderTicketView(ticket.Id, ticket.Preparation.ToString(), ticket.Document.ToString()),
            OrderActions.Available(order, payment),
            order.CreatedAt,
            order.UpdatedAt);
}

public sealed record OrderCustomerView(string? Name, string? Phone, string? Table);

public sealed record OrderAddressView(string Street, string Number, string? Complement, string District);

public sealed record OrderDetailItem(
    Guid ProductId,
    string Name,
    int Quantity,
    long UnitPriceInCents,
    long TotalInCents,
    IReadOnlyList<OrderDetailOption> Options,
    string? Notes)
{
    public static OrderDetailItem From(OrderItem item) =>
        new(
            item.ProductId,
            item.ProductName,
            item.Quantity,
            Money.ToCents(item.UnitPrice),
            Money.ToCents(item.Total),
            item.Options.Select(option => new OrderDetailOption(option.GroupName, option.OptionName, option.Kind.ToString(), Money.ToCents(option.PriceDelta))).ToList(),
            item.Notes);
}

public sealed record OrderDetailOption(string GroupName, string Name, string Kind, long PriceDeltaInCents);

public sealed record OrderPaymentView(string Method, string? Status, long AmountInCents, long? ChangeForInCents, DateTimeOffset? PaidAt);

public sealed record OrderTicketView(Guid Id, string Preparation, string Document);

public enum OrderAction
{
    Accept,
    Reject,
    Cancel,
    Complete,
}

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
