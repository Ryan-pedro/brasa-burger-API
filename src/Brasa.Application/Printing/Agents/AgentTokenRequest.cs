namespace Brasa.Application.Printing.Agents;

public sealed record AgentTokenRequest(Guid? AgentId, string? Secret);
