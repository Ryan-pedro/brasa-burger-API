namespace Brasa.Application.Identity;

public enum PasswordCheckOutcome
{
    Success,
    InvalidCredentials,
    LockedOut,
}
