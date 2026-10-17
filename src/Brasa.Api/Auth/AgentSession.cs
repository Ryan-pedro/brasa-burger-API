namespace Brasa.Api.Auth;

internal static class AgentSession
{
    public static RouteGroupBuilder RequireAgentSession(this RouteGroupBuilder group) =>
        group.RequireAuthorization(AgentAuth.Scheme).AddEndpointFilter<AgentSessionFilter>();
}
