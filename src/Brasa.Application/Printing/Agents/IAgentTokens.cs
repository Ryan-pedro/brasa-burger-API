namespace Brasa.Application.Printing.Agents;

public interface IAgentTokens
{
    AgentTokenResponse Issue(Guid agentId, DateTimeOffset now);

    Guid? ReadAgentId(string token);
}
