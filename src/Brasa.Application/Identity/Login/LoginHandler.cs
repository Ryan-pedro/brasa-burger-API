using Brasa.Application.Identity.Session;

namespace Brasa.Application.Identity.Login;

public sealed record LoginRequest(string? Email, string? Password);

public enum LoginFailure
{
    InvalidCredentials,
    AccountLocked,
    AccountDisabled,
}

public sealed record LoginResult(SessionResponse? Session, LoginFailure? Failure)
{
    public static LoginResult Succeeded(SessionResponse session) => new(session, null);

    public static LoginResult Failed(LoginFailure failure) => new(null, failure);
}

public sealed class LoginHandler(IUserAccounts accounts)
{
    public async Task<LoginResult> HandleAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            return LoginResult.Failed(LoginFailure.InvalidCredentials);
        }

        var check = await accounts.CheckPasswordAsync(request.Email.Trim(), request.Password, cancellationToken);

        if (check.Outcome == PasswordCheckOutcome.LockedOut)
        {
            return LoginResult.Failed(LoginFailure.AccountLocked);
        }

        if (check.Outcome != PasswordCheckOutcome.Success || check.User is null)
        {
            return LoginResult.Failed(LoginFailure.InvalidCredentials);
        }

        if (!check.User.IsActive)
        {
            return LoginResult.Failed(LoginFailure.AccountDisabled);
        }

        return LoginResult.Succeeded(SessionResponse.Create(check.User));
    }
}
