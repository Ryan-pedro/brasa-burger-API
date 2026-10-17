using Brasa.Application.Ordering.Tracking;

namespace Brasa.Application.Ordering.Checkout;

public sealed record CreatedOrder(OrderTrackingResponse Order, bool Replayed);
