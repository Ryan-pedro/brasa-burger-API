namespace Brasa.Application.Shared.Idempotency;

public sealed class IdempotencyRecord
{
    public const int KeyMaxLength = 100;
    public static readonly TimeSpan Retention = TimeSpan.FromHours(24);

    private IdempotencyRecord()
    {
    }

    public IdempotencyRecord(string key, string requestHash, Guid resultId, DateTimeOffset now)
    {
        Key = key;
        RequestHash = requestHash;
        ResultId = resultId;
        CreatedAt = now;
    }

    public string Key { get; private set; } = null!;

    public string RequestHash { get; private set; } = null!;

    public Guid ResultId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
}
