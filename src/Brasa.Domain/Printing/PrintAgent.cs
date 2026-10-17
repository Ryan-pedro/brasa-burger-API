using System.Security.Cryptography;
using Brasa.Domain.Shared;

namespace Brasa.Domain.Printing;

public sealed class PrintAgent : AggregateRoot
{
    public const int NameMaxLength = 60;
    public const int VersionMaxLength = 20;
    public const int LastErrorMaxLength = 300;
    public static readonly TimeSpan OfflineAfter = TimeSpan.FromSeconds(90);

    private PrintAgent()
    {
    }

    private PrintAgent(string name, string agentVersion, string secretHash, DateTimeOffset now)
    {
        Id = Guid.CreateVersion7();
        Name = FreeText.Required(name, NameMaxLength, nameof(name));
        AgentVersion = FreeText.Required(agentVersion, VersionMaxLength, nameof(agentVersion));
        SecretHash = secretHash;
        PairedAt = now;
        LastSeenAt = now;
        IsOnline = true;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public string AgentVersion { get; private set; } = null!;

    public string SecretHash { get; private set; } = null!;

    public DateTimeOffset PairedAt { get; private set; }

    public DateTimeOffset LastSeenAt { get; private set; }

    public bool IsOnline { get; private set; }

    public int LocalQueueSize { get; private set; }

    public string? LastError { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public bool IsRevoked => RevokedAt is not null;

    public static (PrintAgent Agent, string Secret) Pair(string name, string agentVersion, DateTimeOffset now)
    {
        var secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        return (new PrintAgent(name, agentVersion, Printing.SecretHash.Of(secret), now), secret);
    }

    public bool HasSecret(string secret) => !IsRevoked && Printing.SecretHash.Matches(secret, SecretHash);

    public void RecordHeartbeat(string agentVersion, int localQueueSize, string? lastError, DateTimeOffset now)
    {
        RequireActive();
        ArgumentOutOfRangeException.ThrowIfNegative(localQueueSize);

        AgentVersion = FreeText.Required(agentVersion, VersionMaxLength, nameof(agentVersion));
        LocalQueueSize = localQueueSize;
        LastError = FreeText.Clean(lastError, LastErrorMaxLength, nameof(lastError));
        LastSeenAt = now;

        if (!IsOnline)
        {
            IsOnline = true;
            Raise(new PrintAgentPresenceChanged(this));
        }
    }

    public bool IsSilentSince(DateTimeOffset now) => IsOnline && now - LastSeenAt >= OfflineAfter;

    public void MarkOffline()
    {
        if (IsOnline)
        {
            IsOnline = false;
            Raise(new PrintAgentPresenceChanged(this));
        }
    }

    public void Revoke(DateTimeOffset now)
    {
        RequireActive();
        RevokedAt = now;
        IsOnline = false;
        Raise(new PrintAgentPresenceChanged(this));
    }

    private void RequireActive()
    {
        if (IsRevoked)
        {
            throw new InvalidOperationException("O agente foi revogado.");
        }
    }
}
