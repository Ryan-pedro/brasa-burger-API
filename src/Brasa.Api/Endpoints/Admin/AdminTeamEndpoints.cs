using Brasa.Api.Auth;
using Brasa.Api.Problems;
using Brasa.Application.Identity.Team;
using Permissions = Brasa.Application.Identity.Permissions;

namespace Brasa.Api.Endpoints.Admin;

internal static class AdminTeamEndpoints
{
    public static RouteGroupBuilder MapAdminTeamEndpoints(this RouteGroupBuilder secured)
    {
        var team = secured.MapGroup("/team").RequirePermission(Permissions.TeamManage);

        team.MapGet(string.Empty, async (GetTeamHandler handler, CancellationToken cancellationToken) =>
                Results.Ok(await handler.HandleAsync(cancellationToken)))
            .WithName("AdminGetTeam");

        team.MapPost("/invitations", async (InvitationRequest request, CreateInvitationHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(request, cancellationToken)).Match(invitation => Results.Created($"/api/v1/admin/invitations/{invitation.Token}", invitation)))
            .WithName("AdminCreateInvitation");

        team.MapPost("/invitations/{id:guid}/revoke", async (Guid id, RevokeInvitationHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(id, cancellationToken)).Match(value => Results.Ok(value)))
            .WithName("AdminRevokeInvitation");

        team.MapPut("/members/{id:guid}/role", async (Guid id, MemberRoleRequest request, ChangeMemberRoleHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(id, request, cancellationToken)).Match(value => Results.Ok(value)))
            .WithName("AdminChangeMemberRole");

        team.MapPut("/members/{id:guid}/status", async (Guid id, MemberStatusRequest request, SetMemberStatusHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(id, request, cancellationToken)).Match(value => Results.Ok(value)))
            .WithName("AdminSetMemberStatus");

        return secured;
    }
}
