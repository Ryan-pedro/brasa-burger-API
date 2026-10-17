using Brasa.Domain.Shared;

namespace Brasa.Domain.Printing;

public sealed class PrintJob : AggregateRoot
{
    private readonly List<PrintAttempt> _attempts = [];

    private PrintJob()
    {
    }

    public PrintJob(Guid printerId, Guid? ticketId, PrintJobReason reason, PrintDocument document, DateTimeOffset now)
    {
        if (printerId == Guid.Empty)
        {
            throw new ArgumentException("A tarefa precisa de uma impressora.", nameof(printerId));
        }

        if (ticketId is null && reason != PrintJobReason.Test)
        {
            throw new ArgumentException("Só a impressão de teste não tem comanda.", nameof(ticketId));
        }

        ArgumentNullException.ThrowIfNull(document);

        Id = Guid.CreateVersion7();
        PrinterId = printerId;
        TicketId = ticketId;
        Reason = reason;
        Document = document;
        Status = PrintJobStatus.Pending;
        CreatedAt = now;
        NextAttemptAt = now;
        ExpiresAt = reason == PrintJobReason.Automatic ? now + PrintJobPolicy.AutomaticValidity : null;

        Raise(new PrintJobChanged(this));
    }

    public Guid Id { get; private set; }

    public Guid PrinterId { get; private set; }

    public Guid? TicketId { get; private set; }

    public PrintJobReason Reason { get; private set; }

    public PrintDocument Document { get; private set; } = null!;

    public PrintJobStatus Status { get; private set; }

    public bool PossibleDuplicate { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset NextAttemptAt { get; private set; }

    public DateTimeOffset? ExpiresAt { get; private set; }

    public DateTimeOffset? LeasedUntil { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public string? LastError { get; private set; }

    public IReadOnlyList<PrintAttempt> Attempts => _attempts;

    public PrintAttempt? CurrentAttempt => Status == PrintJobStatus.Leased ? _attempts.LastOrDefault(attempt => attempt.IsOpen) : null;

    public bool MayHaveBeenPrinted => Status is PrintJobStatus.Printed or PrintJobStatus.Leased || _attempts.Any(attempt => attempt.Outcome == PrintAttemptOutcome.Unknown);

    public bool CanBeLeasedAt(DateTimeOffset now) =>
        Status == PrintJobStatus.Pending && NextAttemptAt <= now && !IsExpiredAt(now);

    public PrintAttempt Lease(Guid agentId, DateTimeOffset now)
    {
        if (!CanBeLeasedAt(now))
        {
            throw new InvalidOperationException("A tarefa não está disponível para reserva.");
        }

        var attempt = new PrintAttempt(agentId, _attempts.Count + 1, now);
        _attempts.Add(attempt);
        Status = PrintJobStatus.Leased;
        LeasedUntil = now + PrintJobPolicy.LeaseDuration;

        Raise(new PrintJobChanged(this));
        return attempt;
    }

    public void ReportPrinted(Guid attemptId, DateTimeOffset now)
    {
        var attempt = FindAttempt(attemptId);
        if (attempt.IsOpen)
        {
            attempt.Finish(PrintAttemptOutcome.Printed, null, now);
        }

        if (Status is PrintJobStatus.Printed or PrintJobStatus.Cancelled)
        {
            return;
        }

        Status = PrintJobStatus.Printed;
        LeasedUntil = null;
        CompletedAt = now;
        LastError = null;

        Raise(new PrintJobChanged(this));
    }

    public void ReportFailed(Guid attemptId, string? error, DateTimeOffset now) =>
        CloseCurrentAttempt(attemptId, PrintAttemptOutcome.Failed, error, now);

    public void ReportUnknownOutcome(Guid attemptId, DateTimeOffset now)
    {
        CloseCurrentAttempt(attemptId, PrintAttemptOutcome.Unknown, "Resultado desconhecido: o agente parou durante a impressão.", now);
        PossibleDuplicate = true;
    }

    public bool ExpireLease(DateTimeOffset now)
    {
        if (Status != PrintJobStatus.Leased || LeasedUntil > now)
        {
            return false;
        }

        CloseCurrentAttempt(CurrentAttempt!.Id, PrintAttemptOutcome.LeaseExpired, "A reserva venceu sem resultado.", now);
        return true;
    }

    public bool AbandonIfExpired(DateTimeOffset now)
    {
        if (Status != PrintJobStatus.Pending || !IsExpiredAt(now))
        {
            return false;
        }

        Abandon("A tarefa venceu antes de ser impressa.", now);
        return true;
    }

    public void Cancel(DateTimeOffset now)
    {
        if (Status is not (PrintJobStatus.Pending or PrintJobStatus.Leased))
        {
            return;
        }

        CurrentAttempt?.Finish(PrintAttemptOutcome.Unknown, "Tarefa cancelada durante a impressão.", now);
        Status = PrintJobStatus.Cancelled;
        LeasedUntil = null;
        CompletedAt = now;

        Raise(new PrintJobChanged(this));
    }

    private bool IsExpiredAt(DateTimeOffset now) => ExpiresAt <= now;

    private PrintAttempt FindAttempt(Guid attemptId) =>
        _attempts.FirstOrDefault(attempt => attempt.Id == attemptId)
        ?? throw new InvalidOperationException("Tentativa desconhecida para esta tarefa.");

    private void CloseCurrentAttempt(Guid attemptId, PrintAttemptOutcome outcome, string? error, DateTimeOffset now)
    {
        var attempt = CurrentAttempt;
        if (attempt is null || attempt.Id != attemptId)
        {
            throw new InvalidOperationException("A tentativa não é a reserva atual desta tarefa.");
        }

        attempt.Finish(outcome, error, now);
        LastError = attempt.Error;
        LeasedUntil = null;

        if (_attempts.Count >= PrintJobPolicy.MaxAttempts || IsExpiredAt(now))
        {
            Abandon(LastError, now);
            return;
        }

        Status = PrintJobStatus.Pending;
        NextAttemptAt = now + PrintJobPolicy.RetryDelayAfter(_attempts.Count);

        Raise(new PrintJobChanged(this));
    }

    private void Abandon(string? error, DateTimeOffset now)
    {
        Status = PrintJobStatus.Abandoned;
        LastError = error;
        CompletedAt = now;

        Raise(new PrintJobChanged(this));
    }
}
