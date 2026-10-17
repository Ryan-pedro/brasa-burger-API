using System.Text.Encodings.Web;
using Brasa.Api.Realtime;
using Brasa.Application.Printing.Agents;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Brasa.Api.Auth;

internal sealed class AgentAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IAgentTokens tokens)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    private const string BearerPrefix = "Bearer ";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var token = ReadToken();
        if (token is null)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var agentId = tokens.ReadAgentId(token);
        return Task.FromResult(agentId is { } id
            ? AuthenticateResult.Success(new AuthenticationTicket(AgentAuth.CreatePrincipal(id), AgentAuth.Scheme))
            : AuthenticateResult.Fail("Token de agente inválido ou vencido."));
    }

    private string? ReadToken()
    {
        var header = Request.Headers.Authorization.ToString();
        if (header.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return header[BearerPrefix.Length..].Trim();
        }

        var isHubRequest = Request.Path.StartsWithSegments(AgentHub.Path);
        return isHubRequest ? Request.Query[AgentAuth.HubTokenQueryParameter].FirstOrDefault() : null;
    }
}
