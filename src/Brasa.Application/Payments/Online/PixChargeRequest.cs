namespace Brasa.Application.Payments.Online;

public sealed record PixChargeRequest(Guid PaymentId, decimal Amount, DateTimeOffset ExpiresAt, string Description);
