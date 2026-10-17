namespace Brasa.Domain.Payments;

public sealed class PaymentWebhookEvent
{
    public const int EventIdMaxLength = 120;
    public const int ErrorMaxLength = 500;
    public const int MaxAttempts = 10;

    private PaymentWebhookEvent()
    {
    }

    public PaymentWebhookEvent(string provider, string eventId, string chargeId, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(eventId);
        ArgumentException.ThrowIfNullOrWhiteSpace(chargeId);

        if (eventId.Length > EventIdMaxLength || chargeId.Length > Payment.ChargeIdMaxLength || provider.Length > Payment.ProviderMaxLength)
        {
            throw new ArgumentException("Identificadores do aviso de pagamento longos demais.");
        }

        Id = Guid.CreateVersion7();
        Provider = provider;
        EventId = eventId;
        ChargeId = chargeId;
        ReceivedAt = now;
    }

    public Guid Id { get; private set; }

    public string Provider { get; private set; } = null!;

    public string EventId { get; private set; } = null!;

    public string ChargeId { get; private set; } = null!;

    public DateTimeOffset ReceivedAt { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    public int Attempts { get; private set; }

    public string? LastError { get; private set; }

    public bool IsWaiting => ProcessedAt is null && Attempts < MaxAttempts;

    public void MarkProcessed(DateTimeOffset now)
    {
        Attempts++;
        ProcessedAt = now;
        LastError = null;
    }

    public void RecordFailure(string error)
    {
        Attempts++;
        LastError = error.Length > ErrorMaxLength ? error[..ErrorMaxLength] : error;
    }
}
