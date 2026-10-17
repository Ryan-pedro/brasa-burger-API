using System.Security.Cryptography;
using Brasa.Domain.Shared;

namespace Brasa.Domain.Printing;

public sealed class PairingCode
{
    public const int CodeLength = 8;
    public static readonly TimeSpan Validity = TimeSpan.FromMinutes(10);

    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    private PairingCode()
    {
    }

    private PairingCode(string codeHash, Guid? createdBy, DateTimeOffset now)
    {
        Id = Guid.CreateVersion7();
        CodeHash = codeHash;
        CreatedBy = createdBy;
        CreatedAt = now;
        ExpiresAt = now + Validity;
    }

    public Guid Id { get; private set; }

    public string CodeHash { get; private set; } = null!;

    public Guid? CreatedBy { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? UsedAt { get; private set; }

    public static (PairingCode PairingCode, string PlainCode) Generate(Guid? createdBy, DateTimeOffset now)
    {
        var plainCode = RandomNumberGenerator.GetString(Alphabet, CodeLength);
        return (new PairingCode(SecretHash.Of(plainCode), createdBy, now), plainCode);
    }

    public static string Normalize(string code) =>
        new(code.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());

    public bool IsUsableAt(DateTimeOffset now) => UsedAt is null && now < ExpiresAt;

    public void MarkUsed(DateTimeOffset now)
    {
        if (!IsUsableAt(now))
        {
            throw new InvalidOperationException("O código de pareamento já foi usado ou expirou.");
        }

        UsedAt = now;
    }
}
