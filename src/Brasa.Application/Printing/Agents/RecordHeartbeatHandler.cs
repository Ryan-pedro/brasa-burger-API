using Brasa.Application.Shared;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Printing;
using Brasa.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Printing.Agents;

public sealed class RecordHeartbeatHandler(IAppDbContext db, CurrentAgent currentAgent, TimeProvider clock)
{
    public async Task<Result<AgentConfigResponse>> HandleAsync(HeartbeatRequest request, CancellationToken cancellationToken)
    {
        var errors = new FieldErrors();
        var version = FreeText.Normalize(request.AgentVersion);

        if (version is null || version.Length > PrintAgent.VersionMaxLength)
        {
            errors.Add("agentVersion", $"Informe a versão do agente, com até {PrintAgent.VersionMaxLength} caracteres.");
        }

        if (request.LocalQueueSize is null or < 0)
        {
            errors.Add("localQueueSize", "Informe o tamanho da fila local.");
        }

        var reports = request.Printers ?? [];
        var statuses = new Dictionary<Guid, (PrinterStatus Status, string? Message)>();
        for (var index = 0; index < reports.Count; index++)
        {
            var report = reports[index];
            if (report.PrinterId is not { } printerId || !EnumText.TryParse<PrinterStatus>(report.Status, out var status))
            {
                errors.Add($"printers[{index}]", $"Informe a impressora e a situação: {EnumText.Names<PrinterStatus>()}.");
                continue;
            }

            statuses[printerId] = (status, report.Message);
        }

        if (errors.Any)
        {
            return errors.ToError();
        }

        var agentId = currentAgent.RequireAgentId();
        var now = clock.GetUtcNow();

        var agent = await db.PrintAgents.FirstAsync(a => a.Id == agentId, cancellationToken);
        agent.RecordHeartbeat(version!, request.LocalQueueSize!.Value, request.LastError, now);

        var printers = await db.Printers
            .Where(printer => printer.AgentId == agentId && statuses.Keys.Contains(printer.Id))
            .ToListAsync(cancellationToken);

        foreach (var printer in printers)
        {
            var (status, message) = statuses[printer.Id];
            printer.ReportStatus(status, message, now);
        }

        await db.SaveChangesAsync(cancellationToken);

        return await db.AgentConfigAsync(agentId, now, cancellationToken);
    }
}
