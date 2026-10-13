using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Catalog.Products;

public sealed class UpdateProductHandler(IAppDbContext db)
{
    public async Task<Result<ProductResponse>> HandleAsync(Guid id, ProductRequest request, CancellationToken cancellationToken)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (product is null)
        {
            return ProductErrors.NotFound;
        }

        var errors = new FieldErrors();
        var valid = ProductRules.Validate(request, errors);
        if (valid is null)
        {
            return errors.ToError();
        }

        if (valid.CategoryId != product.CategoryId
            && !await db.Categories.AnyAsync(c => c.Id == valid.CategoryId, cancellationToken))
        {
            errors.Add("categoryId", "Categoria não encontrada.");
            return errors.ToError();
        }

        product.MoveToCategory(valid.CategoryId);
        product.ChangeDetails(valid.Name, valid.Description, valid.Price, valid.Size, valid.ImageUrl);

        if (request.IsAvailable is { } isAvailable)
        {
            product.SetAvailability(isAvailable);
        }

        if (request.IsFeatured is { } isFeatured)
        {
            product.SetFeatured(isFeatured);
        }

        if (request.SortOrder is { } sortOrder)
        {
            product.MoveTo(sortOrder);
        }

        await db.SaveChangesAsync(cancellationToken);

        return ProductResponse.From(product);
    }
}
