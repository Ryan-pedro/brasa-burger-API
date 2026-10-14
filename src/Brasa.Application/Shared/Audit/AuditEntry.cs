namespace Brasa.Application.Shared.Audit;

public sealed class AuditEntry
{
    public const int ActionMaxLength = 60;

    private AuditEntry()
    {
    }

    public AuditEntry(string action, Guid? subjectId, Guid? userId, string? data, DateTimeOffset now)
    {
        Id = Guid.CreateVersion7();
        Action = action;
        SubjectId = subjectId;
        UserId = userId;
        Data = data;
        OccurredAt = now;
    }

    public Guid Id { get; private set; }

    public string Action { get; private set; } = null!;

    public Guid? SubjectId { get; private set; }

    public Guid? UserId { get; private set; }

    public string? Data { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }
}
