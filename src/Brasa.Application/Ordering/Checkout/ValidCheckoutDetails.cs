using Brasa.Domain.Ordering;
using Brasa.Domain.Payments;

namespace Brasa.Application.Ordering.Checkout;

internal sealed record ValidCheckoutDetails(OrderCustomer Customer, DeliveryAddress? Address, string? Notes, PaymentMethod PaymentMethod, decimal? ChangeFor);
