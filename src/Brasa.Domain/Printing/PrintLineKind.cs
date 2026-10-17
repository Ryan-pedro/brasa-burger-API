using System.Text.Json.Serialization;

namespace Brasa.Domain.Printing;

[JsonConverter(typeof(JsonStringEnumConverter<PrintLineKind>))]
public enum PrintLineKind
{
    Text,
    Separator,
    Feed,
    Cut,
}
