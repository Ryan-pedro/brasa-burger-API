using Brasa.Application.Shared.Results;
using Brasa.Domain.Catalog;

namespace Brasa.Application.Catalog.Categories;

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
