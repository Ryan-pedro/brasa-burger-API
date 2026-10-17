using System.Security.Cryptography;
using Brasa.Domain.Shared;

namespace Brasa.Domain.Identity;

public sealed class StaffInvitation
{
    public const int EmailMaxLength = 256;
    public const int DisplayNameMaxLength = 120;
    public const int TokenLength = 32;
    public static readonly TimeSpan Validity = TimeSpan.FromDays(3);

    private StaffInvitation()
    {
    }

    private StaffInvitation(string email, string displayName, StaffRole role, string tokenHash, Guid? createdBy, DateTimeOffset now)
    {
        Id = Guid.CreateVersion7();
        Email = FreeText.Required(email, EmailMaxLength, nameof(email)).ToLowerInvariant();
        DisplayName = FreeText.Required(displayName, DisplayNameMaxLength, nameof(displayName));
        Role = role;
        TokenHash = tokenHash;
        CreatedBy = createdBy;
        CreatedAt = now;
        ExpiresAt = now + Validity;
    }

    public Guid Id { get; private set; }

    public string Email { get; private set; } = null!;

    public string DisplayName { get; private set; } = null!;

    public StaffRole Role { get; private set; }

    public string TokenHash { get; private set; } = null!;

    public Guid? CreatedBy { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? AcceptedAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public static (StaffInvitation Invitation, string PlainToken) Create(string email, string displayName, StaffRole role, Guid? createdBy, DateTimeOffset now)
    {
        var plainToken = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(TokenLength / 2));
        return (new StaffInvitation(email, displayName, role, SecretHash.Of(plainToken), createdBy, now), plainToken);
    }

    public bool IsPendingAt(DateTimeOffset now) => AcceptedAt is null && RevokedAt is null && now < ExpiresAt;

    public void Accept(DateTimeOffset now)
    {
        RequirePending(now);
        AcceptedAt = now;
    }

    public void Revoke(DateTimeOffset now)
    {
        RequirePending(now);
        RevokedAt = now;
    }

    private void RequirePending(DateTimeOffset now)
    {
        if (!IsPendingAt(now))
        {
            throw new InvalidOperationException("O convite já foi usado, revogado ou expirou.");
        }
    }
}
