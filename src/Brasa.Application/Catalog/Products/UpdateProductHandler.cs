using Brasa.Application.Shared;
using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Catalog.Products;

public sealed class UpdateProductHandler(IAppDbContext db, AuditTrail audit)
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
        }

        if (request.OptionGroupIds is { } optionGroupIds && !await db.AllExistAsync(optionGroupIds, cancellationToken))
        {
            errors.Add("optionGroupIds", "Grupo de opções não encontrado.");
        }

        if (errors.Any)
        {
            return errors.ToError();
        }

        if (valid.Price != product.Price)
        {
            audit.Record("product.price_changed", product.Id, new { from = Money.ToCents(product.Price), to = Money.ToCents(valid.Price) });
        }

        product.MoveToCategory(valid.CategoryId);
        product.ChangeDetails(valid.Name, valid.Description, valid.Price, valid.Size, valid.ImageUrl);

        if (request.IsAvailable is { } isAvailable && isAvailable != product.IsAvailable)
        {
            product.SetAvailability(isAvailable);
            audit.Record("product.availability_changed", product.Id, new { isAvailable });
        }

        if (request.IsFeatured is { } isFeatured)
        {
            product.SetFeatured(isFeatured);
        }

        if (request.SortOrder is { } sortOrder)
        {
            product.MoveTo(sortOrder);
        }

        if (request.OptionGroupIds is { } groupIds)
        {
            product.SetOptionGroups(groupIds);
        }

        await db.SaveChangesAsync(cancellationToken);

        return ProductResponse.From(product);
    }
}
