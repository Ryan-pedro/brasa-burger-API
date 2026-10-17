using Brasa.Domain.Catalog;

namespace Brasa.Application.Catalog.Categories;

public sealed record CategoryResponse(Guid Id, string Name, string Slug, int SortOrder, bool IsActive, int ProductCount, Guid? ParentId)
{
    public static CategoryResponse From(Category category, int productCount) =>
        new(category.Id, category.Name, category.Slug, category.SortOrder, category.IsActive, productCount, category.ParentId);
}
