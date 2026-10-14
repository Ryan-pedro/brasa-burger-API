namespace Brasa.Api.Auth;

internal static class StaffSession
{
    public static RouteGroupBuilder RequireStaffSession(this RouteGroupBuilder group) =>
        group.RequireAuthorization(AdminAuth.Scheme).AddEndpointFilter<AdminSessionFilter>();
}
