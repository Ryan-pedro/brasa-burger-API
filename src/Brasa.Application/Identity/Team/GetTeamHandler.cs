using Brasa.Application.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Identity.Team;

public sealed class GetTeamHandler(IUserAccounts accounts, IAppDbContext db, TimeProvider clock)
{
    public async Task<TeamResponse> HandleAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var members = await accounts.ListAsync(cancellationToken);
        var invitations = await db.StaffInvitations
            .AsNoTracking()
            .Where(invitation => invitation.AcceptedAt == null && invitation.RevokedAt == null && invitation.ExpiresAt > now)
            .OrderBy(invitation => invitation.CreatedAt)
            .ToListAsync(cancellationToken);

        return new TeamResponse(
            members.Select(TeamMemberResponse.From).ToList(),
            invitations.Select(PendingInvitationResponse.From).ToList());
    }
}
