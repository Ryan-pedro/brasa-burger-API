namespace Brasa.Domain.Catalog;

/// <summary>Um item vendido no cardápio.</summary>
public sealed class Product
{
    public const int NameMaxLength = 120;
    public const int DescriptionMaxLength = 500;
    public const int SizeMaxLength = 20;
    public const int ImageUrlMaxLength = 300;

    /// <summary>Maior preço aceito, em reais.</summary>
    public const decimal MaxPrice = 9999.99m;

    // Usado pelo EF Core ao materializar a entidade.
    private Product()
    {
    }

    public Product(Guid categoryId, string name, string? description, decimal price, string? size, string? imageUrl, int sortOrder)
    {
        Id = Guid.CreateVersion7();
        MoveToCategory(categoryId);
        ChangeDetails(name, description, price, size, imageUrl);
        MoveTo(sortOrder);
        IsAvailable = true;
    }

    public Guid Id { get; private set; }

    public Guid CategoryId { get; private set; }

    public string Name { get; private set; } = null!;

    /// <summary>Pode ser vazia, nunca nula.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Em reais, com no máximo duas casas decimais.</summary>
    public decimal Price { get; private set; }

    /// <summary>Tamanho exibido junto do nome, como "350ml". Opcional.</summary>
    public string? Size { get; private set; }

    public string? ImageUrl { get; private set; }

    /// <summary>Produto indisponível continua no cardápio, mas não pode ser pedido.</summary>
    public bool IsAvailable { get; private set; }

    /// <summary>Aparece em "Mais pedidos" no Storefront.</summary>
    public bool IsFeatured { get; private set; }

    public int SortOrder { get; private set; }

    public void ChangeDetails(string name, string? description, decimal price, string? size, string? imageUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var trimmedName = name.Trim();
        if (trimmedName.Length > NameMaxLength)
        {
            throw new ArgumentException($"O nome do produto pode ter no máximo {NameMaxLength} caracteres.", nameof(name));
        }

        var trimmedDescription = description?.Trim() ?? string.Empty;
        if (trimmedDescription.Length > DescriptionMaxLength)
        {
            throw new ArgumentException($"A descrição pode ter no máximo {DescriptionMaxLength} caracteres.", nameof(description));
        }

        if (price <= 0 || price > MaxPrice || decimal.Round(price, 2) != price)
        {
            throw new ArgumentOutOfRangeException(nameof(price), price, $"O preço deve ser maior que zero, até {MaxPrice}, com no máximo duas casas.");
        }

        var trimmedSize = Optional(size, SizeMaxLength, nameof(size));
        var trimmedImage = Optional(imageUrl, ImageUrlMaxLength, nameof(imageUrl));

        Name = trimmedName;
        Description = trimmedDescription;
        Price = price;
        Size = trimmedSize;
        ImageUrl = trimmedImage;
    }

    public void MoveToCategory(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
        {
            throw new ArgumentException("O produto precisa de uma categoria.", nameof(categoryId));
        }

        CategoryId = categoryId;
    }

    public void MoveTo(int sortOrder)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(sortOrder);
        SortOrder = sortOrder;
    }

    public void SetAvailability(bool isAvailable) => IsAvailable = isAvailable;

    public void SetFeatured(bool isFeatured) => IsFeatured = isFeatured;

    private static string? Optional(string? value, int maxLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"O campo pode ter no máximo {maxLength} caracteres.", parameterName);
        }

        return trimmed;
    }
}
