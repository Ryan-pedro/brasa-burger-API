using Brasa.Application.Printing.Agents;
using Brasa.Application.Shared;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Printing.Jobs;

public sealed class ReportPrintJobResultHandler(IAppDbContext db, CurrentAgent currentAgent, TimeProvider clock)
{
    public async Task<Result<PrintJobResponse>> HandleAsync(Guid jobId, PrintJobResultRequest request, CancellationToken cancellationToken)
    {
        var errors = new FieldErrors();
        if (request.AttemptId is null || request.AttemptId == Guid.Empty)
        {
            errors.Add("attemptId", "Informe a tentativa recebida na reserva.");
        }

        if (!EnumText.TryParse<ReportedPrintOutcome>(request.Outcome, out var outcome))
        {
            errors.Add("outcome", $"Resultados aceitos: {EnumText.Names<ReportedPrintOutcome>()}.");
        }

        if (errors.Any)
        {
            return errors.ToError();
        }

        var agentId = currentAgent.RequireAgentId();
        var agentPrinterIds = db.Printers.Where(printer => printer.AgentId == agentId).Select(printer => printer.Id);

        var job = await db.PrintJobs.FirstOrDefaultAsync(j => j.Id == jobId && agentPrinterIds.Contains(j.PrinterId), cancellationToken);
        var attempt = job?.Attempts.FirstOrDefault(a => a.Id == request.AttemptId && a.AgentId == agentId);
        if (job is null || attempt is null)
        {
            return PrintingErrors.JobNotFound;
        }

        var now = clock.GetUtcNow();
        if (outcome == ReportedPrintOutcome.Printed)
        {
            job.ReportPrinted(attempt.Id, now);
        }
        else if (attempt.IsOpen)
        {
            if (job.CurrentAttempt?.Id != attempt.Id)
            {
                return PrintingErrors.AttemptNotCurrent;
            }

            if (outcome == ReportedPrintOutcome.Failed)
            {
                job.ReportFailed(attempt.Id, request.Error, now);
            }
            else
            {
                job.ReportUnknownOutcome(attempt.Id, now);
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        return PrintJobResponse.From(job);
    }
}
