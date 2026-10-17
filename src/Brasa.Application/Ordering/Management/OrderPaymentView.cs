namespace Brasa.Application.Ordering.Management;

public sealed record OrderPaymentView(string Method, string? Status, long AmountInCents, long? ChangeForInCents, DateTimeOffset? PaidAt);
