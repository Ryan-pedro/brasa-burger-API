using Brasa.Application.Shared.Persistence;
using Brasa.Domain.Identity;
using Brasa.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Identity.Team;

internal static class InvitationQueries
{
    public static async Task<StaffInvitation?> PendingInvitationByTokenAsync(this IAppDbContext db, string token, DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (token.Length != StaffInvitation.TokenLength)
        {
            return null;
        }

        var tokenHash = SecretHash.Of(token.ToLowerInvariant());
        var invitation = await db.StaffInvitations.FirstOrDefaultAsync(i => i.TokenHash == tokenHash, cancellationToken);
        return invitation is not null && invitation.IsPendingAt(now) ? invitation : null;
    }
}
