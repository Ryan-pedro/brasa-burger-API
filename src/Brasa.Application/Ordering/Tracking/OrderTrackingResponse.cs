using Brasa.Application.Ordering.Shared;
using Brasa.Application.Shared;
using Brasa.Domain.Ordering;

namespace Brasa.Application.Ordering.Tracking;

public sealed record OrderTrackingResponse(
    int Number,
    string TrackingToken,
    string Status,
    string? StatusReason,
    string Type,
    IReadOnlyList<TrackingItem> Items,
    long SubtotalInCents,
    string? DiscountLabel,
    long DiscountInCents,
    long DeliveryFeeInCents,
    long TotalInCents,
    string PaymentMethod,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static OrderTrackingResponse From(Order order) =>
        new(
            order.Number,
            order.TrackingToken,
            order.Status.ToString(),
            order.StatusReason,
            order.Type.ToString(),
            order.Items.Select(item => new TrackingItem(
                item.ProductName,
                item.Quantity,
                Money.ToCents(item.Total),
                item.Options.Select(OptionText.ForCustomer).ToList(),
                item.Notes)).ToList(),
            Money.ToCents(order.Subtotal),
            order.DiscountLabel,
            Money.ToCents(order.DiscountAmount),
            Money.ToCents(order.DeliveryFee),
            Money.ToCents(order.Total),
            order.PaymentMethod.ToString(),
            order.CreatedAt,
            order.UpdatedAt);
}

public sealed record TrackingItem(string Name, int Quantity, long TotalInCents, IReadOnlyList<string> Options, string? Notes);
