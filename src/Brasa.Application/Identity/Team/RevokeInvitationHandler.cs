using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Identity.Team;

public sealed class RevokeInvitationHandler(IAppDbContext db, AuditTrail audit, TimeProvider clock)
{
    public async Task<Result<PendingInvitationResponse>> HandleAsync(Guid invitationId, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var invitation = await db.StaffInvitations.FirstOrDefaultAsync(i => i.Id == invitationId, cancellationToken);
        if (invitation is null || !invitation.IsPendingAt(now))
        {
            return TeamErrors.InvitationNotFound;
        }

        invitation.Revoke(now);
        audit.Record("team.invitation_revoked", invitation.Id, new { invitation.Email });
        await db.SaveChangesAsync(cancellationToken);

        return PendingInvitationResponse.From(invitation);
    }
}
