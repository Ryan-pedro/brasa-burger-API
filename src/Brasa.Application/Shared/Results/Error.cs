namespace Brasa.Application.Shared.Results;

/// <summary>
/// Falha esperada de um caso de uso. O <see cref="Code"/> é estável: o frontend decide por ele.
/// </summary>
public sealed record Error(
    ErrorKind Kind,
    string Code,
    string Message,
    IReadOnlyDictionary<string, string[]>? Fields = null,
    IReadOnlyDictionary<string, object?>? Extensions = null)
{
    public static Error Validation(IReadOnlyDictionary<string, string[]> fields) =>
        new(ErrorKind.Validation, "validation_failed", "Alguns campos precisam de correção.", fields);

    public static Error NotFound(string code, string message) => new(ErrorKind.NotFound, code, message);

    public static Error Conflict(string code, string message) => new(ErrorKind.Conflict, code, message);

    public static Error Rule(string code, string message) => new(ErrorKind.Rule, code, message);

    public static Error Unavailable(string code, string message) => new(ErrorKind.Unavailable, code, message);
}
