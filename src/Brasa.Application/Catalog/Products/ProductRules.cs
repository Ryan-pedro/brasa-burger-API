using Brasa.Application.Shared;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Catalog;

namespace Brasa.Application.Catalog.Products;

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

        if (request.OptionGroupIds is { } groupIds
            && (groupIds.Count > Product.MaxOptionGroups || groupIds.Contains(Guid.Empty) || groupIds.Distinct().Count() != groupIds.Count))
        {
            errors.Add("optionGroupIds", $"Escolha até {Product.MaxOptionGroups} grupos, sem repetir.");
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
