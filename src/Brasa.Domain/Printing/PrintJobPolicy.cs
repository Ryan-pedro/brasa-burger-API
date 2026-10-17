namespace Brasa.Domain.Printing;

public static class PrintJobPolicy
{
    public const int MaxAttempts = 5;

    public static readonly TimeSpan LeaseDuration = TimeSpan.FromSeconds(60);

    public static readonly TimeSpan AutomaticValidity = TimeSpan.FromMinutes(30);

    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(15),
        TimeSpan.FromSeconds(45),
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(5),
    ];

    public static TimeSpan RetryDelayAfter(int attemptCount) =>
        RetryDelays[Math.Clamp(attemptCount - 1, 0, RetryDelays.Length - 1)];
}
