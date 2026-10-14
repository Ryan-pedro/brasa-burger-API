using Brasa.Application.Shared.Results;

namespace Brasa.Application.Ordering.Checkout;

public sealed class QuoteHandler(CartBuilder cart)
{
    public async Task<Result<QuoteResponse>> HandleAsync(QuoteRequest request, CancellationToken cancellationToken)
    {
        var priced = await cart.BuildAsync(request.Type, request.Items, cancellationToken);
        return priced.IsSuccess ? QuoteResponse.From(priced.Value!) : priced.Error!;
    }
}
