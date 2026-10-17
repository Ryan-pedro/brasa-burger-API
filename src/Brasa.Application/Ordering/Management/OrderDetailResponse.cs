using Brasa.Application.Shared;
using Brasa.Domain.Kitchen;
using Brasa.Domain.Ordering;
using Brasa.Domain.Payments;

namespace Brasa.Application.Ordering.Management;

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
                payment?.PaidAt,
                payment?.ExpiresAt),
            ticket is null ? null : new OrderTicketView(ticket.Id, ticket.Preparation.ToString(), ticket.Document.ToString()),
            OrderActions.Available(order, payment),
            order.CreatedAt,
            order.UpdatedAt);
}
