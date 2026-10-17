namespace Brasa.Application.Payments.Online;

public sealed record PixChargeStatus(string ChargeId, PixChargeState State, decimal? PaidAmount, string Currency);
