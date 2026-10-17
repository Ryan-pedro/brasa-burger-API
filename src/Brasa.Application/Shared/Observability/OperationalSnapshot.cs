namespace Brasa.Application.Shared.Observability;

public sealed record OperationalSnapshot(
    double OutboxOldestPendingSeconds,
    long PendingPrintJobs,
    double PrintOldestPendingSeconds,
    long OnlinePrintAgents,
    long WaitingPixPayments)
{
    public static readonly OperationalSnapshot Empty = new(0, 0, 0, 0, 0);
}
