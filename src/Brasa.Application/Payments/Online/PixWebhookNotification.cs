namespace Brasa.Application.Payments.Online;

public sealed record PixWebhookNotification(string EventId, string ChargeId);
