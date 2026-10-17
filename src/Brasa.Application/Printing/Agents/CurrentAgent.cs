namespace Brasa.Application.Printing.Agents;

public sealed class CurrentAgent
{
    public Guid? AgentId { get; private set; }

    public void Set(Guid agentId)
    {
        if (AgentId is not null)
        {
            throw new InvalidOperationException("O agente corrente já foi definido e não pode ser alterado.");
        }

        AgentId = agentId;
    }

    public Guid RequireAgentId() =>
        AgentId ?? throw new InvalidOperationException("Esta operação exige um agente autenticado.");
}
