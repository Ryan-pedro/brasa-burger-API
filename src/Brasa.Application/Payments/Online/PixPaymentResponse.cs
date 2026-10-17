using Brasa.Application.Shared;
using Brasa.Domain.Payments;

namespace Brasa.Application.Payments.Online;

public sealed record PixPaymentResponse(string Status, long AmountInCents, string? PixCopyPaste, DateTimeOffset? ExpiresAt, DateTimeOffset? PaidAt)
{
    public static PixPaymentResponse From(Payment payment) =>
        new(
            payment.Status.ToString(),
            Money.ToCents(payment.Amount),
            payment.Status == PaymentStatus.Pending ? payment.PixCopyPaste : null,
            payment.ExpiresAt,
            payment.PaidAt);
}
