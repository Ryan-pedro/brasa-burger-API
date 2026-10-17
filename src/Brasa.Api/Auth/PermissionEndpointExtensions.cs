namespace Brasa.Api.Auth;

internal static class PermissionEndpointExtensions
{
    /// <summary>Use dentro de um grupo que já tenha o <see cref="AdminSessionFilter"/>.</summary>
    public static TBuilder RequirePermission<TBuilder>(this TBuilder builder, string permission)
        where TBuilder : IEndpointConventionBuilder =>
        builder.AddEndpointFilter(new PermissionFilter(permission));
}
