using Brasa.Api.Auth;
using Brasa.Api.Http;
using Brasa.Api.Problems;
using Brasa.Application.Identity.Team;
using Microsoft.AspNetCore.Authentication;

namespace Brasa.Api.Endpoints.Admin;

internal static class AdminInvitationEndpoints
{
    public static RouteGroupBuilder MapAdminInvitationEndpoints(this RouteGroupBuilder admin)
    {
        var invitations = admin.MapGroup("/invitations").AllowAnonymous().RequireRateLimiting(RateLimits.Invitations);

        invitations.MapGet("/{token}", async (string token, GetInvitationHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(token, cancellationToken)).Match(value => Results.Ok(value)))
            .WithName("AdminGetInvitation");

        invitations.MapPost("/{token}/accept", async (string token, AcceptInvitationRequest request, AcceptInvitationHandler handler, HttpContext http, CancellationToken cancellationToken) =>
            {
                var accepted = await handler.HandleAsync(token, request, cancellationToken);
                if (accepted.IsSuccess)
                {
                    await http.SignInAsync(AdminAuth.Scheme, AdminAuth.CreatePrincipal(accepted.Value!.User.Id));
                }

                return accepted.Match(session => Results.Ok(session));
            })
            .WithName("AdminAcceptInvitation");

        return admin;
    }
}
