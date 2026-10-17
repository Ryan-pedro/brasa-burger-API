using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Auth;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;

namespace Brasa.Application.Identity.Team;

public sealed class ChangeMemberRoleHandler(IUserAccounts accounts, IAppDbContext db, CurrentUser currentUser, AuditTrail audit)
{
    public async Task<Result<TeamMemberResponse>> HandleAsync(Guid memberId, MemberRoleRequest request, CancellationToken cancellationToken)
    {
        var errors = new FieldErrors();
        var role = AssignableRoles.Parse(request.Role, errors);
        if (errors.Any)
        {
            return errors.ToError();
        }

        var member = await accounts.FindAsync(memberId, cancellationToken);
        if (MemberChangeRules.Check(member, currentUser) is { } refused)
        {
            return refused;
        }

        if (member!.Role != role)
        {
            await accounts.ChangeRoleAsync(member.Id, role!.Value, cancellationToken);
            audit.Record("team.role_changed", member.Id, new { from = member.Role.ToString(), to = role.Value.ToString() });
            await db.SaveChangesAsync(cancellationToken);
        }

        return TeamMemberResponse.From(member with { Role = role!.Value });
    }
}
