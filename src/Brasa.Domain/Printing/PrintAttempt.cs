namespace Brasa.Domain.Printing;

public sealed class PrintAttempt
{
    public const int ErrorMaxLength = 300;

    private PrintAttempt()
    {
    }

    internal PrintAttempt(Guid agentId, int number, DateTimeOffset now)
    {
        Id = Guid.CreateVersion7();
        AgentId = agentId;
        Number = number;
        StartedAt = now;
    }

    public Guid Id { get; private set; }

    public Guid AgentId { get; private set; }

    public int Number { get; private set; }

    public DateTimeOffset StartedAt { get; private set; }

    public DateTimeOffset? FinishedAt { get; private set; }

    public PrintAttemptOutcome? Outcome { get; private set; }

    public string? Error { get; private set; }

    public bool IsOpen => Outcome is null;

    internal void Finish(PrintAttemptOutcome outcome, string? error, DateTimeOffset now)
    {
        Outcome = outcome;
        Error = error is { Length: > ErrorMaxLength } ? error[..ErrorMaxLength] : error;
        FinishedAt = now;
    }
}
