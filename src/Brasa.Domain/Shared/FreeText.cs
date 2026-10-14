namespace Brasa.Domain.Shared;

public static class FreeText
{
    public static string? Normalize(string? value)
    {
        var cleaned = value is null ? string.Empty : new string(value.Where(character => !char.IsControl(character)).ToArray()).Trim();
        return cleaned.Length == 0 ? null : cleaned;
    }

    public static string? Clean(string? value, int maxLength, string parameterName)
    {
        var cleaned = Normalize(value);
        if (cleaned?.Length > maxLength)
        {
            throw new ArgumentException($"O campo pode ter no máximo {maxLength} caracteres.", parameterName);
        }

        return cleaned;
    }

    public static string Required(string? value, int maxLength, string parameterName) =>
        Clean(value, maxLength, parameterName) ?? throw new ArgumentException("O campo é obrigatório.", parameterName);
}
