using Brasa.Application.Shared.Results;

namespace Brasa.Api.Problems;

/// <summary>
/// Respostas de erro no formato Problem Details, sempre com um "code" estável
/// que o frontend usa para decidir o que fazer.
/// </summary>
internal static class ApiProblems
{
    public static IResult Create(int statusCode, string title, string code) =>
        TypedResults.Problem(
            statusCode: statusCode,
            title: title,
            extensions: new Dictionary<string, object?> { ["code"] = code });

    /// <summary>Converte a falha de um caso de uso na resposta HTTP correspondente.</summary>
    public static IResult From(Error error)
    {
        var statusCode = error.Kind switch
        {
            ErrorKind.Validation => StatusCodes.Status400BadRequest,
            ErrorKind.NotFound => StatusCodes.Status404NotFound,
            ErrorKind.Conflict => StatusCodes.Status409Conflict,
            ErrorKind.Rule => StatusCodes.Status422UnprocessableEntity,
            ErrorKind.Unavailable => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status400BadRequest,
        };

        var extensions = new Dictionary<string, object?> { ["code"] = error.Code };
        if (error.Fields is not null)
        {
            // Mensagens por campo, com o nome do campo como está no JSON: { "priceInCents": ["..."] }
            extensions["errors"] = error.Fields;
        }

        foreach (var (key, value) in error.Extensions ?? new Dictionary<string, object?>())
        {
            extensions[key] = value;
        }

        return TypedResults.Problem(statusCode: statusCode, title: error.Message, extensions: extensions);
    }

    /// <summary>Sucesso vira <paramref name="onSuccess"/>; falha vira Problem Details.</summary>
    public static IResult Match<T>(this Result<T> result, Func<T, IResult> onSuccess) =>
        result.IsSuccess ? onSuccess(result.Value!) : From(result.Error!);
}
