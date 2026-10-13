using Brasa.Api.Auth;
using Brasa.Api.Problems;
using Brasa.Application.Identity.Login;
using Brasa.Application.Identity.Session;
using Microsoft.AspNetCore.Authentication;

namespace Brasa.Api.Endpoints.Admin;

/// <summary>Superfície do painel administrativo: entrada, saída e sessão.</summary>
public static class AdminAuthEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder api)
    {
        // Toda a superfície exige o cabeçalho anti-CSRF nas requisições que alteram estado.
        var admin = api.MapGroup("/admin").AddEndpointFilter<CsrfHeaderFilter>();

        admin.MapPost("/auth/login", async (LoginRequest request, LoginHandler handler, HttpContext http, CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(request, cancellationToken);

                if (result.Session is null)
                {
                    return result.Failure switch
                    {
                        LoginFailure.AccountLocked => ApiProblems.Create(
                            StatusCodes.Status403Forbidden,
                            "Conta bloqueada temporariamente por excesso de tentativas.",
                            "account_locked"),
                        LoginFailure.AccountDisabled => ApiProblems.Create(
                            StatusCodes.Status403Forbidden,
                            "Este usuário está desativado.",
                            "account_disabled"),
                        _ => ApiProblems.Create(
                            StatusCodes.Status401Unauthorized,
                            "E-mail ou senha inválidos.",
                            "invalid_credentials"),
                    };
                }

                await http.SignInAsync(AdminAuth.Scheme, AdminAuth.CreatePrincipal(result.Session.User.Id));
                return Results.Ok(result.Session);
            })
            .WithName("AdminLogin")
            .AllowAnonymous();

        admin.MapPost("/auth/logout", async (HttpContext http) =>
            {
                await http.SignOutAsync(AdminAuth.Scheme);
                return Results.NoContent();
            })
            .WithName("AdminLogout")
            .AllowAnonymous();

        // Daqui para baixo: sessão obrigatória, revalidada no banco a cada requisição.
        var secured = admin.MapGroup(string.Empty)
            .RequireAuthorization(AdminAuth.Scheme)
            .AddEndpointFilter<AdminSessionFilter>();

        secured.MapGet("/session", async (GetSessionHandler handler, CancellationToken cancellationToken) =>
            {
                var session = await handler.HandleAsync(cancellationToken);
                return session is null
                    ? ApiProblems.Create(StatusCodes.Status401Unauthorized, "Sessão encerrada. Entre de novo.", "session_expired")
                    : Results.Ok(session);
            })
            .WithName("AdminGetSession");

        return api;
    }
}
