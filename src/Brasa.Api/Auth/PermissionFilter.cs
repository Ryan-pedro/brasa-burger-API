using Brasa.Api.Problems;
using Brasa.Application.Shared.Auth;

namespace Brasa.Api.Auth;

/// <summary>
/// Exige uma permissão do papel do usuário. Roda depois do <see cref="AdminSessionFilter"/>,
/// que já releu o usuário no banco e preencheu o <see cref="CurrentUser"/>.
/// </summary>
internal sealed class PermissionFilter(string permission) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var user = context.HttpContext.RequestServices.GetRequiredService<CurrentUser>();

        if (user.Role is not { } role || !Brasa.Application.Identity.Permissions.For(role).Contains(permission))
        {
            return ApiProblems.Create(StatusCodes.Status403Forbidden, "Seu papel não permite esta ação.", "permission_denied");
        }

        return await next(context);
    }
}

internal static class PermissionEndpointExtensions
{
    /// <summary>Use dentro de um grupo que já tenha o <see cref="AdminSessionFilter"/>.</summary>
    public static TBuilder RequirePermission<TBuilder>(this TBuilder builder, string permission)
        where TBuilder : IEndpointConventionBuilder =>
        builder.AddEndpointFilter(new PermissionFilter(permission));
}
