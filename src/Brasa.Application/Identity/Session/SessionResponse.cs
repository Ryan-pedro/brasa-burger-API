namespace Brasa.Application.Identity.Session;

/// <summary>O que o painel precisa saber sobre a sessão: quem é e o que pode fazer.</summary>
public sealed record SessionResponse(SessionUser User, string Role, IReadOnlyList<string> Permissions)
{
    public static SessionResponse Create(UserInfo user) =>
        new(
            new SessionUser(user.Id, user.Email, user.DisplayName),
            user.Role.ToString(),
            Brasa.Application.Identity.Permissions.For(user.Role));
}
