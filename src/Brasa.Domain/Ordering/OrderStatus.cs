namespace Brasa.Domain.Ordering;

public enum OrderStatus
{
    Placed,
    Confirmed,
    InPreparation,
    Ready,
    OutForDelivery,
    Completed,
    Cancelled,
    Rejected,
}
