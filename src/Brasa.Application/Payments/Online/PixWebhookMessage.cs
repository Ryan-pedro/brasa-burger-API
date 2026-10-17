namespace Brasa.Application.Payments.Online;

public sealed record PixWebhookMessage(string Body, IReadOnlyDictionary<string, string> Headers);
