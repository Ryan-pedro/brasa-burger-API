using System.Security.Cryptography;
using System.Text;

namespace Brasa.Domain.Printing;

public static class SecretHash
{
    public static string Of(string secret) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(secret)));

    public static bool Matches(string secret, string expectedHash) =>
        CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(Of(secret)), Encoding.ASCII.GetBytes(expectedHash));
}
