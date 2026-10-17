namespace Brasa.Application.Printing.Agents;

public sealed record AgentTokenResponse(string AccessToken, DateTimeOffset ExpiresAt);
