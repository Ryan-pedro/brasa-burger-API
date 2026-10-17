using Brasa.Api.Problems;
using Brasa.Application.Printing.Agents;

namespace Brasa.Api.Auth;

internal sealed class AgentSessionFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var http = context.HttpContext;
        var services = http.RequestServices;

        if (!AgentAuth.TryReadAgentId(http.User, out var agentId)
            || !await services.GetRequiredService<ActivePrintAgents>().IsActiveAsync(agentId, http.RequestAborted))
        {
            return ApiProblems.Create(StatusCodes.Status401Unauthorized, "Agente não autorizado. Obtenha um novo token ou pareie de novo.", "agent_unauthorized");
        }

        services.GetRequiredService<CurrentAgent>().Set(agentId);

        return await next(context);
    }
}
