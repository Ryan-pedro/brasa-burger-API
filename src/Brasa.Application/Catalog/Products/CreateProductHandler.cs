using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Catalog.Products;

public sealed class CreateProductHandler(IAppDbContext db)
{
    public async Task<Result<ProductResponse>> HandleAsync(ProductRequest request, CancellationToken cancellationToken)
    {
        var errors = new FieldErrors();
        var valid = ProductRules.Validate(request, errors);
        if (valid is null)
        {
            return errors.ToError();
        }

        if (!await db.Categories.AnyAsync(c => c.Id == valid.CategoryId, cancellationToken))
        {
            errors.Add("categoryId", "Categoria não encontrada.");
        }

        var optionGroupIds = request.OptionGroupIds ?? [];
        if (!await db.AllExistAsync(optionGroupIds, cancellationToken))
        {
            errors.Add("optionGroupIds", "Grupo de opções não encontrado.");
        }

        if (errors.Any)
        {
            return errors.ToError();
        }

        // Sem posição informada, o produto entra no fim da categoria.
        var sortOrder = request.SortOrder
            ?? (await db.Products
                .Where(p => p.CategoryId == valid.CategoryId)
                .MaxAsync(p => (int?)p.SortOrder, cancellationToken) ?? -1) + 1;

        var product = new Product(valid.CategoryId, valid.Name, valid.Description, valid.Price, valid.Size, valid.ImageUrl, sortOrder);
        if (request.IsAvailable == false)
        {
            product.SetAvailability(false);
        }

        if (request.IsFeatured == true)
        {
            product.SetFeatured(true);
        }

        product.SetOptionGroups(optionGroupIds);

        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);

        return ProductResponse.From(product);
    }
}
