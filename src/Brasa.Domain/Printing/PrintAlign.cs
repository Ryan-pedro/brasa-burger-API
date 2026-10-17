using System.Text.Json.Serialization;

namespace Brasa.Domain.Printing;

[JsonConverter(typeof(JsonStringEnumConverter<PrintAlign>))]
public enum PrintAlign
{
    Left,
    Center,
    Right,
}
