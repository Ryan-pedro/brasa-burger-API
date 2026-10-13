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
}
