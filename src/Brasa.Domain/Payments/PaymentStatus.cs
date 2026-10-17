namespace Brasa.Domain.Payments;

public enum PaymentStatus
{
    Pending,
    Paid,
    Expired,
    PaidLate,
    AmountMismatch,
}
