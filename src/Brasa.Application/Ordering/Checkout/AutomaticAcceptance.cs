using Brasa.Domain.Ordering;
using Brasa.Domain.StoreSettings;

namespace Brasa.Application.Ordering.Checkout;

public static class AutomaticAcceptance
{
    public static void ApplyTo(Order order, Store store, DateTimeOffset now)
    {
        if (order.Status == OrderStatus.Placed && !store.ManualAcceptance)
        {
            order.Confirm(now);
        }
    }
}
