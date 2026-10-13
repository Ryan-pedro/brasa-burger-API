namespace Brasa.Domain.StoreSettings;

/// <summary>
/// Os dados da loja. O sistema atende uma única loja, então existe um único registro.
/// </summary>
public sealed class Store
{
    /// <summary>Identificador fixo do único registro da loja.</summary>
    public static readonly Guid SingletonId = new("00000000-0000-0000-0000-000000000001");

    public const int NameMaxLength = 120;

    // Usado pelo EF Core ao materializar a entidade.
    private Store()
    {
    }

    public Store(string name, string timeZoneId)
    {
        Id = SingletonId;
        Rename(name);
        ChangeTimeZone(timeZoneId);
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    /// <summary>Fuso horário da loja no padrão IANA (ex.: "America/Sao_Paulo").</summary>
    public string TimeZoneId { get; private set; } = null!;

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var trimmed = name.Trim();
        if (trimmed.Length > NameMaxLength)
        {
            throw new ArgumentException($"O nome da loja pode ter no máximo {NameMaxLength} caracteres.", nameof(name));
        }

        Name = trimmed;
    }

    public void ChangeTimeZone(string timeZoneId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZoneId);
        TimeZoneId = timeZoneId;
    }
}
