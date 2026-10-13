namespace Brasa.Domain.Catalog;

/// <summary>Uma seção do cardápio, como "Hambúrgueres" ou "Bebidas".</summary>
public sealed class Category
{
    public const int NameMaxLength = 60;

    // Usado pelo EF Core ao materializar a entidade.
    private Category()
    {
    }

    public Category(string name, int sortOrder)
    {
        Id = Guid.CreateVersion7();
        Rename(name);
        MoveTo(sortOrder);
        IsActive = true;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    /// <summary>Derivado do nome; único no cardápio.</summary>
    public string Slug { get; private set; } = null!;

    public int SortOrder { get; private set; }

    /// <summary>Categoria inativa some do cardápio público, com todos os seus produtos.</summary>
    public bool IsActive { get; private set; }

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var trimmed = name.Trim();
        if (trimmed.Length > NameMaxLength)
        {
            throw new ArgumentException($"O nome da categoria pode ter no máximo {NameMaxLength} caracteres.", nameof(name));
        }

        var slug = Slugs.From(trimmed);
        if (slug.Length == 0)
        {
            throw new ArgumentException("O nome da categoria precisa ter letras ou números.", nameof(name));
        }

        Name = trimmed;
        Slug = slug;
    }

    public void MoveTo(int sortOrder)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(sortOrder);
        SortOrder = sortOrder;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
