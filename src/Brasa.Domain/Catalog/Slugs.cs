using System.Globalization;
using System.Text;

namespace Brasa.Domain.Catalog;

/// <summary>Gera identificadores legíveis para endereços: "Porções & Extras" vira "porcoes-extras".</summary>
public static class Slugs
{
    public static string From(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var builder = new StringBuilder(text.Length);
        var lastWasDash = false;

        foreach (var character in text.Normalize(NormalizationForm.FormD))
        {
            // Acentos viram caracteres separados na forma decomposta; são descartados.
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsAsciiLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));
                lastWasDash = false;
            }
            else if (builder.Length > 0 && !lastWasDash)
            {
                builder.Append('-');
                lastWasDash = true;
            }
        }

        return builder.ToString().TrimEnd('-');
    }
}
