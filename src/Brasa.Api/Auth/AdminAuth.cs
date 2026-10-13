using System.Security.Claims;

namespace Brasa.Api.Auth;

/// <summary>Nomes e ajudantes da sessão do painel administrativo.</summary>
internal static class AdminAuth
{
    /// <summary>Esquema de autenticação e nome da política de autorização do painel.</summary>
    public const string Scheme = "admin";

    public const string UserIdClaim = "sub";

    /// <summary>Em produção, o prefixo __Host- prende o cookie ao host exato e exige HTTPS.</summary>
    public const string SecureCookieName = "__Host-brasa-admin";

    /// <summary>Em desenvolvimento e nos testes a API roda em HTTP, onde o prefixo __Host- não funciona.</summary>
    public const string DevelopmentCookieName = "brasa-admin";

    /// <summary>
    /// A sessão guarda só quem é o usuário. Papel e situação são relidos do banco
    /// a cada requisição, para que uma desativação tenha efeito imediato.
    /// </summary>
    public static ClaimsPrincipal CreatePrincipal(Guid userId) =>
        new(new ClaimsIdentity([new Claim(UserIdClaim, userId.ToString())], Scheme));

    public static bool TryReadUserId(ClaimsPrincipal principal, out Guid userId) =>
        Guid.TryParse(principal.FindFirstValue(UserIdClaim), out userId);
}
