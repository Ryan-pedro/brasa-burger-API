namespace Brasa.Application.Ordering.Checkout;

public interface IOrderNumbers
{
    Task<int> NextAsync(CancellationToken cancellationToken);
}
