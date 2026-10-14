namespace Brasa.Application.Shared.Results;

/// <summary>Acumula erros por campo. Os nomes dos campos seguem o JSON da API (camelCase).</summary>
public sealed class FieldErrors
{
    private readonly Dictionary<string, List<string>> _errors = new(StringComparer.Ordinal);

    public bool Any => _errors.Count > 0;

    public void Add(string field, string message)
    {
        if (!_errors.TryGetValue(field, out var messages))
        {
            messages = [];
            _errors[field] = messages;
        }

        messages.Add(message);
    }

    public Error ToError() => Error.Validation(Snapshot());

    public Error ToConflict(string code, string message) => new(ErrorKind.Conflict, code, message, Snapshot());

    private Dictionary<string, string[]> Snapshot() =>
        _errors.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray(), StringComparer.Ordinal);
}
