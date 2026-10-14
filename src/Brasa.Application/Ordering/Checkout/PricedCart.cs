using Brasa.Domain.Ordering;
using Brasa.Domain.StoreSettings;

namespace Brasa.Application.Ordering.Checkout;

public sealed record PricedCart(Store Store, OrderType Type, IReadOnlyList<CartLine> Lines, PriceBreakdown Price);
