using System.Security.Claims;

namespace Brasa.Api.Auth;

internal static class AgentAuth
{
    public const string Scheme = "agent";

    public const string AgentIdClaim = "agent_id";

    public const string HubTokenQueryParameter = "access_token";

    public static ClaimsPrincipal CreatePrincipal(Guid agentId) =>
        new(new ClaimsIdentity([new Claim(AgentIdClaim, agentId.ToString())], Scheme));

    public static bool TryReadAgentId(ClaimsPrincipal principal, out Guid agentId) =>
        Guid.TryParse(principal.FindFirstValue(AgentIdClaim), out agentId);
}
