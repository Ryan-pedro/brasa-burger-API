using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Catalog.Categories;

public sealed class MoveCategoryHandler(IAppDbContext db)
{
    public async Task<Result<CategoryResponse>> HandleAsync(Guid id, CategoryParentRequest request, CancellationToken cancellationToken)
    {
        var category = await db.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (category is null)
        {
            return CategoryErrors.NotFound;
        }

        if (request.ParentId is { } parentId)
        {
            var errors = new FieldErrors();
            var parent = await db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == parentId, cancellationToken);

            if (parent is null || parent.Id == id)
            {
                errors.Add("parentId", "Categoria principal não encontrada.");
            }
            else if (parent.ParentId is not null)
            {
                errors.Add("parentId", "Escolha uma categoria principal, não uma subcategoria.");
            }
            else if (await db.Categories.AnyAsync(c => c.ParentId == id, cancellationToken))
            {
                errors.Add("parentId", "Esta categoria tem subcategorias e não pode virar subcategoria.");
            }

            if (errors.Any)
            {
                return errors.ToError();
            }
        }

        category.MoveUnder(request.ParentId);
        await db.SaveChangesAsync(cancellationToken);

        var productCount = await db.Products.CountAsync(product => product.CategoryId == id, cancellationToken);
        return CategoryResponse.From(category, productCount);
    }
}
