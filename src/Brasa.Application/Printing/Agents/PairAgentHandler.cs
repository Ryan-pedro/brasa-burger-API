using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Printing;
using Brasa.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Printing.Agents;

public sealed class PairAgentHandler(IAppDbContext db, AuditTrail audit, TimeProvider clock)
{
    public async Task<Result<PairAgentResponse>> HandleAsync(PairAgentRequest request, CancellationToken cancellationToken)
    {
        var errors = new FieldErrors();
        var name = FreeText.Normalize(request.Name);
        var version = FreeText.Normalize(request.AgentVersion);

        if (string.IsNullOrWhiteSpace(request.Code))
        {
            errors.Add("code", "Informe o código de pareamento.");
        }

        if (name is null || name.Length > PrintAgent.NameMaxLength)
        {
            errors.Add("name", $"Informe o nome do computador, com até {PrintAgent.NameMaxLength} caracteres.");
        }

        if (version is null || version.Length > PrintAgent.VersionMaxLength)
        {
            errors.Add("agentVersion", $"Informe a versão do agente, com até {PrintAgent.VersionMaxLength} caracteres.");
        }

        if (errors.Any)
        {
            return errors.ToError();
        }

        var now = clock.GetUtcNow();
        var codeHash = SecretHash.Of(PairingCode.Normalize(request.Code!));
        var pairingCode = await db.PairingCodes.FirstOrDefaultAsync(code => code.CodeHash == codeHash, cancellationToken);
        if (pairingCode is null || !pairingCode.IsUsableAt(now))
        {
            return PrintingErrors.PairingCodeInvalid;
        }

        pairingCode.MarkUsed(now);
        var (agent, secret) = PrintAgent.Pair(name!, version!, now);

        db.PrintAgents.Add(agent);
        audit.Record("printing.agent_paired", agent.Id, new { agent.Name, agent.AgentVersion }, pairingCode.CreatedBy);
        await db.SaveChangesAsync(cancellationToken);

        return new PairAgentResponse(agent.Id, secret);
    }
}
