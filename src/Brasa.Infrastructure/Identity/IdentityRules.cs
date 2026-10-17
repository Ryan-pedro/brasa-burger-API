namespace Brasa.Infrastructure.Identity;

internal static class IdentityRules
{
    public const int PasswordMinimumLength = 10;
    public const int PasswordMinimumUniqueCharacters = 4;
    public const int MaxFailedLoginAttempts = 5;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
}
