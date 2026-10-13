using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Catalog.Categories;

public sealed class UpdateCategoryHandler(IAppDbContext db)
{
    public async Task<Result<CategoryResponse>> HandleAsync(Guid id, CategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (category is null)
        {
            return CategoryErrors.NotFound;
        }

        var errors = new FieldErrors();
        var name = request.Name is null ? null : CategoryRules.ValidateName(request.Name, errors);
        CategoryRules.ValidateSortOrder(request.SortOrder, errors);
        if (errors.Any)
        {
            return errors.ToError();
        }

        if (name is not null)
        {
            var slug = Slugs.From(name);
            var taken = await db.Categories.AnyAsync(c => c.Slug == slug && c.Id != id, cancellationToken);
            if (taken)
            {
                return CategoryErrors.NameTaken;
            }

            category.Rename(name);
        }

        if (request.SortOrder is { } sortOrder)
        {
            category.MoveTo(sortOrder);
        }

        if (request.IsActive is { } isActive)
        {
            if (isActive)
            {
                category.Activate();
            }
            else
            {
                category.Deactivate();
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        var productCount = await db.Products.CountAsync(product => product.CategoryId == id, cancellationToken);
        return CategoryResponse.From(category, productCount);
    }
}
