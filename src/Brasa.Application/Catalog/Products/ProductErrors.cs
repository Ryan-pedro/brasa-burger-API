using Brasa.Application.Shared.Results;

namespace Brasa.Application.Catalog.Products;

public static class ProductErrors
{
    public static readonly Error NotFound = Error.NotFound("product_not_found", "Produto não encontrado.");
}
