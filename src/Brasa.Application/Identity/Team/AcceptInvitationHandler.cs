using Brasa.Application.Identity.Session;
using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;

namespace Brasa.Application.Identity.Team;

public sealed class AcceptInvitationHandler(IUserAccounts accounts, IAppDbContext db, AuditTrail audit, TimeProvider clock)
{
    public async Task<Result<SessionResponse>> HandleAsync(string token, AcceptInvitationRequest request, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var invitation = await db.PendingInvitationByTokenAsync(token, now, cancellationToken);
        if (invitation is null)
        {
            return TeamErrors.InvitationNotFound;
        }

        if (string.IsNullOrEmpty(request.Password))
        {
            var errors = new FieldErrors();
            errors.Add("password", "Crie uma senha.");
            return errors.ToError();
        }

        invitation.Accept(now);

        var created = await accounts.CreateAsync(new NewStaffAccount(invitation.Email, invitation.DisplayName, invitation.Role), request.Password, cancellationToken);
        if (!created.IsSuccess)
        {
            return created.Error!;
        }

        var user = created.Value!;
        audit.Record("team.invitation_accepted", invitation.Id, new { invitation.Email, role = invitation.Role.ToString() }, user.Id);
        await db.SaveChangesAsync(cancellationToken);

        return SessionResponse.Create(user);
    }
}
