namespace Brasa.Application.Shared;

public static class EnumText
{
    public static bool TryParse<TEnum>(string? text, out TEnum value)
        where TEnum : struct, Enum
    {
        var name = Enum.GetNames<TEnum>().FirstOrDefault(candidate => string.Equals(candidate, text?.Trim(), StringComparison.OrdinalIgnoreCase));
        value = name is null ? default : Enum.Parse<TEnum>(name);
        return name is not null;
    }

    public static string Names<TEnum>()
        where TEnum : struct, Enum =>
        string.Join(", ", Enum.GetNames<TEnum>());
}
