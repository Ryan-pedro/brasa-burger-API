namespace Brasa.Application.Identity.Team;

public sealed record TeamMemberResponse(Guid Id, string Email, string DisplayName, string Role, bool IsActive)
{
    public static TeamMemberResponse From(UserInfo user) =>
        new(user.Id, user.Email, user.DisplayName, user.Role.ToString(), user.IsActive);
}
