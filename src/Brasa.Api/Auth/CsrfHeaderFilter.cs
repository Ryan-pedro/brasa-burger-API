using Brasa.Api.Problems;

namespace Brasa.Api.Auth;

/// <summary>
/// Proteção contra requisições forjadas por outros sites (CSRF) nas superfícies com sessão em cookie.
/// </summary>
/// <remarks>
/// Toda requisição que altera estado precisa trazer um cabeçalho próprio. Um formulário
/// de outro site não consegue enviá-lo, e um script de outra origem só conseguiria com
/// uma liberação de CORS que esta API não concede. Além disso, se o navegador informar
/// a origem, ela precisa ser o próprio host.
/// </remarks>
internal sealed class CsrfHeaderFilter : IEndpointFilter
{
    public const string HeaderName = "X-Brasa-Csrf";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.HttpContext.Request;

        if (HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method) || HttpMethods.IsOptions(request.Method))
        {
            return await next(context);
        }

        if (!request.Headers.ContainsKey(HeaderName) || !IsSameOrigin(request))
        {
            return ApiProblems.Create(
                StatusCodes.Status403Forbidden,
                "Requisição recusada pela proteção contra falsificação.",
                "csrf_validation_failed");
        }

        return await next(context);
    }

    private static bool IsSameOrigin(HttpRequest request)
    {
        var origin = request.Headers.Origin.ToString();
        if (string.IsNullOrEmpty(origin))
        {
            // Clientes que não são navegadores não enviam Origin; o cabeçalho próprio já foi exigido.
            return true;
        }

        return Uri.TryCreate(origin, UriKind.Absolute, out var uri)
            && string.Equals(uri.Authority, request.Host.Value, StringComparison.OrdinalIgnoreCase);
    }
}
