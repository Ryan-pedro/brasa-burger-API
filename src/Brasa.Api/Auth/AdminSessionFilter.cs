using Brasa.Api.Problems;
using Brasa.Application.Identity;
using Brasa.Application.Shared.Auth;
using Microsoft.AspNetCore.Authentication;

namespace Brasa.Api.Auth;

/// <summary>
/// Roda em toda rota autenticada do painel. Relê o usuário no banco e só então define o
/// usuário corrente. Um usuário desativado ou removido perde a sessão na requisição seguinte.
/// </summary>
internal sealed class AdminSessionFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var http = context.HttpContext;
        var services = http.RequestServices;

        if (!AdminAuth.TryReadUserId(http.User, out var userId))
        {
            return await EndSessionAsync(http);
        }

        var user = await services.GetRequiredService<IUserAccounts>().FindAsync(userId, http.RequestAborted);

        if (user is not { IsActive: true })
        {
            return await EndSessionAsync(http);
        }

        services.GetRequiredService<CurrentUser>().Set(user.Id, user.Role);

        return await next(context);
    }

    private static async Task<IResult> EndSessionAsync(HttpContext http)
    {
        await http.SignOutAsync(AdminAuth.Scheme);

        return ApiProblems.Create(StatusCodes.Status401Unauthorized, "Sessão encerrada. Entre de novo.", "session_expired");
    }
}
