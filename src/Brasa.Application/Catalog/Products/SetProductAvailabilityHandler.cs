using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Catalog.Products;

/// <summary>A ação mais usada no dia a dia: "acabou o pão brioche".</summary>
public sealed class SetProductAvailabilityHandler(IAppDbContext db)
{
    public async Task<Result<ProductResponse>> HandleAsync(Guid id, AvailabilityRequest request, CancellationToken cancellationToken)
    {
        if (request.IsAvailable is not { } isAvailable)
        {
            var errors = new FieldErrors();
            errors.Add("isAvailable", "Informe se o produto está disponível.");
            return errors.ToError();
        }

        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product is null)
        {
            return ProductErrors.NotFound;
        }

        product.SetAvailability(isAvailable);
        await db.SaveChangesAsync(cancellationToken);

        return ProductResponse.From(product);
    }
}
