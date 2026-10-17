namespace Brasa.Application.Catalog.Products;

/// <summary>Os dados já validados, prontos para o domínio.</summary>
internal sealed record ValidProduct(Guid CategoryId, string Name, string Description, decimal Price, string? Size, string? ImageUrl);
