using Brasa.Application.Shared.Results;
using Brasa.Domain.Catalog;

namespace Brasa.Application.Catalog.Categories;

/// <summary>Corpo de criação e de edição. Campos nulos na edição mantêm o valor atual.</summary>
public sealed record CategoryRequest(string? Name, int? SortOrder, bool? IsActive);

public sealed record CategoryResponse(Guid Id, string Name, string Slug, int SortOrder, bool IsActive, int ProductCount)
{
    public static CategoryResponse From(Category category, int productCount) =>
        new(category.Id, category.Name, category.Slug, category.SortOrder, category.IsActive, productCount);
}

internal static class CategoryRules
{
    public static string? ValidateName(string? name, FieldErrors errors)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add("name", "Informe o nome da categoria.");
            return null;
        }

        var trimmed = name.Trim();
        if (trimmed.Length > Category.NameMaxLength)
        {
            errors.Add("name", $"Use no máximo {Category.NameMaxLength} caracteres.");
            return null;
        }

        if (Slugs.From(trimmed).Length == 0)
        {
            errors.Add("name", "O nome precisa ter letras ou números.");
            return null;
        }

        return trimmed;
    }

    public static void ValidateSortOrder(int? sortOrder, FieldErrors errors)
    {
        if (sortOrder is < 0)
        {
            errors.Add("sortOrder", "A posição não pode ser negativa.");
        }
    }
}
