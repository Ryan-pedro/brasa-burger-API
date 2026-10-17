using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Identity;

namespace Brasa.Application.Identity.Team;

public sealed class InviteOwnerHandler(IUserAccounts accounts, IAppDbContext db, AuditTrail audit, TimeProvider clock)
{
    public async Task<Result<CreatedInvitationResponse>> HandleAsync(string email, string displayName, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        if (await accounts.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            return TeamErrors.MemberExists;
        }

        var (invitation, token) = StaffInvitation.Create(normalizedEmail, displayName, StaffRole.Owner, null, clock.GetUtcNow());
        db.StaffInvitations.Add(invitation);
        audit.Record("team.owner_invited", invitation.Id, new { invitation.Email });
        await db.SaveChangesAsync(cancellationToken);

        return new CreatedInvitationResponse(invitation.Id, invitation.Email, invitation.DisplayName, invitation.Role.ToString(), token, invitation.ExpiresAt);
    }
}
