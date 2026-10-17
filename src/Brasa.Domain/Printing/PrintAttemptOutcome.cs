namespace Brasa.Domain.Printing;

public enum PrintAttemptOutcome
{
    Printed,
    Failed,
    LeaseExpired,
    Unknown,
}
