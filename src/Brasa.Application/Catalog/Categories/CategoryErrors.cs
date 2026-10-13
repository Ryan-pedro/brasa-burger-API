using Brasa.Application.Shared.Results;

namespace Brasa.Application.Catalog.Categories;

public static class CategoryErrors
{
    public static readonly Error NotFound = Error.NotFound("category_not_found", "Categoria não encontrada.");

    public static readonly Error NameTaken = Error.Conflict("category_name_taken", "Já existe uma categoria com esse nome.");
}
