using Brasa.Application.Shared;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Catalog;

namespace Brasa.Application.Catalog.Products;

/// <summary>
/// Corpo de criação e de edição de produto. Preço em centavos (ADR-0012).
/// Na edição, <c>isAvailable</c>, <c>isFeatured</c> e <c>sortOrder</c> nulos mantêm o valor atual.
/// </summary>
public sealed record ProductRequest(
    Guid? CategoryId,
    string? Name,
    string? Description,
    long? PriceInCents,
    string? Size,
    string? ImageUrl,
    bool? IsAvailable,
    bool? IsFeatured,
    int? SortOrder);

public sealed record AvailabilityRequest(bool? IsAvailable);

public sealed record ProductResponse(
    Guid Id,
    Guid CategoryId,
    string Name,
    string Description,
    long PriceInCents,
    string? Size,
    string? ImageUrl,
    bool IsAvailable,
    bool IsFeatured,
    int SortOrder)
{
    public static ProductResponse From(Product product) =>
        new(
            product.Id,
            product.CategoryId,
            product.Name,
            product.Description,
            Money.ToCents(product.Price),
            product.Size,
            product.ImageUrl,
            product.IsAvailable,
            product.IsFeatured,
            product.SortOrder);
}

public static class ProductErrors
{
    public static readonly Error NotFound = Error.NotFound("product_not_found", "Produto não encontrado.");
}

/// <summary>Os dados já validados, prontos para o domínio.</summary>
internal sealed record ValidProduct(Guid CategoryId, string Name, string Description, decimal Price, string? Size, string? ImageUrl);

internal static class ProductRules
{
    private const long MaxPriceInCents = 999_999;

    /// <summary>Valida o corpo; devolve nulo e preenche <paramref name="errors"/> se algo estiver errado.</summary>
    public static ValidProduct? Validate(ProductRequest request, FieldErrors errors)
    {
        if (request.CategoryId is null || request.CategoryId == Guid.Empty)
        {
            errors.Add("categoryId", "Escolha a categoria.");
        }

        var name = request.Name?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            errors.Add("name", "Informe o nome do produto.");
        }
        else if (name.Length > Product.NameMaxLength)
        {
            errors.Add("name", $"Use no máximo {Product.NameMaxLength} caracteres.");
        }

        var description = request.Description?.Trim() ?? string.Empty;
        if (description.Length > Product.DescriptionMaxLength)
        {
            errors.Add("description", $"Use no máximo {Product.DescriptionMaxLength} caracteres.");
        }

        if (request.PriceInCents is null)
        {
            errors.Add("priceInCents", "Informe o preço.");
        }
        else if (request.PriceInCents <= 0)
        {
            errors.Add("priceInCents", "O preço precisa ser maior que zero.");
        }
        else if (request.PriceInCents > MaxPriceInCents)
        {
            errors.Add("priceInCents", "O preço máximo é R$ 9.999,99.");
        }

        if (request.Size?.Trim().Length > Product.SizeMaxLength)
        {
            errors.Add("size", $"Use no máximo {Product.SizeMaxLength} caracteres.");
        }

        if (request.ImageUrl?.Trim().Length > Product.ImageUrlMaxLength)
        {
            errors.Add("imageUrl", $"Use no máximo {Product.ImageUrlMaxLength} caracteres.");
        }

        if (request.SortOrder is < 0)
        {
            errors.Add("sortOrder", "A posição não pode ser negativa.");
        }

        if (errors.Any)
        {
            return null;
        }

        return new ValidProduct(
            request.CategoryId!.Value,
            name!,
            description,
            Money.FromCents(request.PriceInCents!.Value),
            request.Size,
            request.ImageUrl);
    }
}
