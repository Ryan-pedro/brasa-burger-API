namespace Brasa.Application.Shared.Outbox;

public sealed class OutboxMessage
{
    public const int MaxAttempts = 10;
    public const int ErrorMaxLength = 500;

    private OutboxMessage()
    {
    }

    public OutboxMessage(string channel, string type, Guid subjectId, DateTimeOffset now)
    {
        Id = Guid.CreateVersion7();
        Channel = channel;
        Type = type;
        SubjectId = subjectId;
        CreatedAt = now;
    }

    public Guid Id { get; private set; }

    public string Channel { get; private set; } = null!;

    public string Type { get; private set; } = null!;

    public Guid SubjectId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    public int Attempts { get; private set; }

    public string? LastError { get; private set; }

    public void MarkProcessed(DateTimeOffset now)
    {
        Attempts++;
        ProcessedAt = now;
    }

    public void MarkFailed(string error)
    {
        Attempts++;
        LastError = error.Length > ErrorMaxLength ? error[..ErrorMaxLength] : error;
    }
}
