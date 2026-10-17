using System.Security.Cryptography;
using Brasa.Application.Printing.Agents;
using Microsoft.AspNetCore.DataProtection;

namespace Brasa.Api.Auth;

internal sealed class AgentTokens(IDataProtectionProvider dataProtection) : IAgentTokens
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(15);

    private readonly ITimeLimitedDataProtector _protector =
        dataProtection.CreateProtector("Brasa.PrintAgent.AccessToken").ToTimeLimitedDataProtector();

    public AgentTokenResponse Issue(Guid agentId, DateTimeOffset now)
    {
        var expiresAt = now + Lifetime;
        return new AgentTokenResponse(_protector.Protect(agentId.ToString(), expiresAt), expiresAt);
    }

    public Guid? ReadAgentId(string token)
    {
        try
        {
            return Guid.TryParse(_protector.Unprotect(token), out var agentId) ? agentId : null;
        }
        catch (CryptographicException)
        {
            return null;
        }
    }
}
