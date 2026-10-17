using Microsoft.AspNetCore.Identity;

namespace Brasa.Infrastructure.Identity;

internal static class PasswordMessages
{
    public static string For(IdentityError error) => error.Code switch
    {
        nameof(IdentityErrorDescriber.PasswordTooShort) => $"Use pelo menos {IdentityRules.PasswordMinimumLength} caracteres.",
        nameof(IdentityErrorDescriber.PasswordRequiresUniqueChars) => $"Use pelo menos {IdentityRules.PasswordMinimumUniqueCharacters} caracteres diferentes.",
        _ => "Escolha uma senha mais forte.",
    };
}
