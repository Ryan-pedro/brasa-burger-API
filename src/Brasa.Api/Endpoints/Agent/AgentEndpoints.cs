using Brasa.Api.Auth;
using Brasa.Api.Http;
using Brasa.Api.Problems;
using Brasa.Application.Printing.Agents;
using Brasa.Application.Printing.Jobs;

namespace Brasa.Api.Endpoints.Agent;

internal static class AgentEndpoints
{
    public static IEndpointRouteBuilder MapAgentEndpoints(this IEndpointRouteBuilder api)
    {
        var agent = api.MapGroup("/agent").AddEndpointFilter<AgentProtocolFilter>();

        agent.MapPost("/pair", async (PairAgentRequest request, PairAgentHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(request, cancellationToken)).Match(value => Results.Ok(value)))
            .RequireRateLimiting(RateLimits.AgentCredentials)
            .WithName("AgentPair");

        agent.MapPost("/token", async (AgentTokenRequest request, IssueAgentTokenHandler handler, CancellationToken cancellationToken) =>
            {
                var token = await handler.HandleAsync(request, cancellationToken);
                return token is null
                    ? ApiProblems.Create(StatusCodes.Status401Unauthorized, "Credencial do agente inválida ou revogada.", "agent_credentials_invalid")
                    : Results.Ok(token);
            })
            .RequireRateLimiting(RateLimits.AgentCredentials)
            .WithName("AgentToken");

        var session = agent.MapGroup(string.Empty).RequireAgentSession();

        session.MapPost("/heartbeat", async (HeartbeatRequest request, RecordHeartbeatHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(request, cancellationToken)).Match(value => Results.Ok(value)))
            .WithName("AgentHeartbeat");

        session.MapGet("/config", async (GetAgentConfigHandler handler, CancellationToken cancellationToken) =>
                Results.Ok(await handler.HandleAsync(cancellationToken)))
            .WithName("AgentConfig");

        session.MapPost("/jobs/lease", async (LeasePrintJobsRequest request, LeasePrintJobsHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(request, cancellationToken)).Match(value => Results.Ok(value)))
            .WithName("AgentLeaseJobs");

        session.MapPost("/jobs/{id:guid}/result", async (Guid id, PrintJobResultRequest request, ReportPrintJobResultHandler handler, CancellationToken cancellationToken) =>
                (await handler.HandleAsync(id, request, cancellationToken)).Match(value => Results.Ok(value)))
            .WithName("AgentReportJobResult");

        return api;
    }
}
