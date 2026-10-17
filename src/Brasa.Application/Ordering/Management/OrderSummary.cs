namespace Brasa.Application.Ordering.Management;

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
