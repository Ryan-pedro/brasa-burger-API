using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Application.StoreSettings.Shared;

namespace Brasa.Application.Identity.Team;

public sealed class GetInvitationHandler(IAppDbContext db, TimeProvider clock)
{
    public async Task<Result<InvitationPreviewResponse>> HandleAsync(string token, CancellationToken cancellationToken)
    {
        var invitation = await db.PendingInvitationByTokenAsync(token, clock.GetUtcNow(), cancellationToken);
        if (invitation is null)
        {
            return TeamErrors.InvitationNotFound;
        }

        var store = await db.CurrentStoreAsync(cancellationToken);
        return new InvitationPreviewResponse(invitation.Email, invitation.DisplayName, invitation.Role.ToString(), store?.Name ?? string.Empty, invitation.ExpiresAt);
    }
}
