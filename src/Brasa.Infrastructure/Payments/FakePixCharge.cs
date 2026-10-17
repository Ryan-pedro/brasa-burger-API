namespace Brasa.Infrastructure.Payments;

internal sealed class FakePixCharge(string chargeId, decimal amount, DateTimeOffset expiresAt)
{
    public string ChargeId { get; } = chargeId;

    public decimal Amount { get; } = amount;

    public DateTimeOffset ExpiresAt { get; } = expiresAt;

    public decimal? PaidAmount { get; set; }
}
