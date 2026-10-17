namespace Brasa.Api.Endpoints.Development;

public sealed record SimulatePixPaymentRequest(long? PaidAmountInCents, bool? DeliverWebhook);
