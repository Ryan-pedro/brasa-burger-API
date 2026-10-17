using System.Net.Mail;
using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Auth;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Identity;
using Brasa.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Identity.Team;

public sealed class CreateInvitationHandler(IUserAccounts accounts, IAppDbContext db, CurrentUser currentUser, AuditTrail audit, TimeProvider clock)
{
    public async Task<Result<CreatedInvitationResponse>> HandleAsync(InvitationRequest request, CancellationToken cancellationToken)
    {
        var errors = new FieldErrors();
        var email = ValidEmail(request.Email, errors);
        var displayName = FreeText.Normalize(request.DisplayName);
        if (displayName is null || displayName.Length > StaffInvitation.DisplayNameMaxLength)
        {
            errors.Add("displayName", $"Informe o nome, com até {StaffInvitation.DisplayNameMaxLength} caracteres.");
        }

        var role = AssignableRoles.Parse(request.Role, errors);
        if (errors.Any)
        {
            return errors.ToError();
        }

        if (await accounts.EmailExistsAsync(email!, cancellationToken))
        {
            return TeamErrors.MemberExists;
        }

        var now = clock.GetUtcNow();
        var previous = await db.StaffInvitations
            .Where(invitation => invitation.Email == email && invitation.AcceptedAt == null && invitation.RevokedAt == null && invitation.ExpiresAt > now)
            .ToListAsync(cancellationToken);

        foreach (var replaced in previous)
        {
            replaced.Revoke(now);
            audit.Record("team.invitation_revoked", replaced.Id, new { replaced.Email, reason = "replaced" });
        }

        var (invitation, token) = StaffInvitation.Create(email!, displayName!, role!.Value, currentUser.UserId, now);
        db.StaffInvitations.Add(invitation);
        audit.Record("team.invitation_created", invitation.Id, new { invitation.Email, role = invitation.Role.ToString() });
        await db.SaveChangesAsync(cancellationToken);

        return new CreatedInvitationResponse(invitation.Id, invitation.Email, invitation.DisplayName, invitation.Role.ToString(), token, invitation.ExpiresAt);
    }

    private static string? ValidEmail(string? text, FieldErrors errors)
    {
        var email = text?.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(email)
            || email.Length > StaffInvitation.EmailMaxLength
            || !MailAddress.TryCreate(email, out var parsed)
            || parsed.Address != email)
        {
            errors.Add("email", "Informe um e-mail válido.");
            return null;
        }

        return email;
    }
}
