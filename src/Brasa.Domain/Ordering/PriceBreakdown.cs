namespace Brasa.Domain.Ordering;

public sealed record PriceBreakdown(decimal Subtotal, AppliedDiscount? Discount, decimal DeliveryFee)
{
    public decimal DiscountAmount => Discount?.Amount ?? 0m;

    public decimal Total => Math.Max(0m, Subtotal - DiscountAmount + DeliveryFee);
}
