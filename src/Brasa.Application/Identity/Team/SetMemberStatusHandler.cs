using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Auth;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;

namespace Brasa.Application.Identity.Team;

public sealed class SetMemberStatusHandler(IUserAccounts accounts, IAppDbContext db, CurrentUser currentUser, AuditTrail audit)
{
    public async Task<Result<TeamMemberResponse>> HandleAsync(Guid memberId, MemberStatusRequest request, CancellationToken cancellationToken)
    {
        if (request.IsActive is not { } isActive)
        {
            var errors = new FieldErrors();
            errors.Add("isActive", "Informe se a conta fica ativa.");
            return errors.ToError();
        }

        var member = await accounts.FindAsync(memberId, cancellationToken);
        if (MemberChangeRules.Check(member, currentUser) is { } refused)
        {
            return refused;
        }

        if (member!.IsActive != isActive)
        {
            await accounts.SetActiveAsync(member.Id, isActive, cancellationToken);
            audit.Record(isActive ? "team.member_reactivated" : "team.member_deactivated", member.Id);
            await db.SaveChangesAsync(cancellationToken);
        }

        return TeamMemberResponse.From(member with { IsActive = isActive });
    }
}
