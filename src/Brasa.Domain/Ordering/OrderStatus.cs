namespace Brasa.Domain.Ordering;

public enum OrderStatus
{
    AwaitingPayment,
    Placed,
    Confirmed,
    InPreparation,
    Ready,
    OutForDelivery,
    Completed,
    Cancelled,
    Rejected,
    PaymentExpired,
}
