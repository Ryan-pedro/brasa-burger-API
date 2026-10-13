using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Catalog.Categories;

public sealed class CreateCategoryHandler(IAppDbContext db)
{
    public async Task<Result<CategoryResponse>> HandleAsync(CategoryRequest request, CancellationToken cancellationToken)
    {
        var errors = new FieldErrors();
        var name = CategoryRules.ValidateName(request.Name, errors);
        CategoryRules.ValidateSortOrder(request.SortOrder, errors);
        if (errors.Any)
        {
            return errors.ToError();
        }

        var slug = Slugs.From(name!);
        if (await db.Categories.AnyAsync(c => c.Slug == slug, cancellationToken))
        {
            return CategoryErrors.NameTaken;
        }

        // Sem posição informada, a categoria entra no fim do cardápio.
        var sortOrder = request.SortOrder
            ?? (await db.Categories.MaxAsync(c => (int?)c.SortOrder, cancellationToken) ?? -1) + 1;

        var category = new Category(name!, sortOrder);
        if (request.IsActive == false)
        {
            category.Deactivate();
        }

        db.Categories.Add(category);
        await db.SaveChangesAsync(cancellationToken);

        return CategoryResponse.From(category, productCount: 0);
    }
}
