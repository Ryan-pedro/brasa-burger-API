namespace Brasa.Infrastructure.Payments;

internal sealed record FakePixWebhookBody(string EventId, string ChargeId);
