using Brasa.Application.Shared.Auth;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Identity;

namespace Brasa.Application.Identity.Team;

internal static class MemberChangeRules
{
    public static Error? Check(UserInfo? member, CurrentUser currentUser)
    {
        if (member is null)
        {
            return TeamErrors.MemberNotFound;
        }

        if (member.Id == currentUser.UserId)
        {
            return TeamErrors.CannotChangeOwnAccount;
        }

        return member.Role == StaffRole.Owner ? TeamErrors.OwnerAccountProtected : null;
    }
}
