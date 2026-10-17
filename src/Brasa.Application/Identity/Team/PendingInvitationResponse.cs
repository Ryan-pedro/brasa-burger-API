using Brasa.Domain.Identity;

namespace Brasa.Application.Identity.Team;

public sealed record PendingInvitationResponse(Guid Id, string Email, string DisplayName, string Role, DateTimeOffset CreatedAt, DateTimeOffset ExpiresAt)
{
    public static PendingInvitationResponse From(StaffInvitation invitation) =>
        new(invitation.Id, invitation.Email, invitation.DisplayName, invitation.Role.ToString(), invitation.CreatedAt, invitation.ExpiresAt);
}
