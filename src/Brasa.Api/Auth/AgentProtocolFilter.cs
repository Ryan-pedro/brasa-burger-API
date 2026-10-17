using Brasa.Api.Problems;
using Brasa.Application.Printing.Agents;

namespace Brasa.Api.Auth;

internal sealed class AgentProtocolFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var header = context.HttpContext.Request.Headers[AgentProtocol.Header].ToString();

        if (!int.TryParse(header, out var protocolVersion) || protocolVersion < AgentProtocol.Minimum)
        {
            return ApiProblems.Create(
                StatusCodes.Status426UpgradeRequired,
                $"Atualize o agente de impressão. Envie {AgentProtocol.Header} com a versão {AgentProtocol.Minimum} ou maior.",
                "agent_protocol_outdated");
        }

        return await next(context);
    }
}
