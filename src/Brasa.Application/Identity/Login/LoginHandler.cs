using Brasa.Application.Identity.Session;
using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Observability;
using Brasa.Application.Shared.Persistence;

namespace Brasa.Application.Identity.Login;

public sealed class LoginHandler(IUserAccounts accounts, IAppDbContext db, AuditTrail audit, BrasaMetrics metrics)
{
    public async Task<LoginResult> HandleAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            return LoginResult.Failed(LoginFailure.InvalidCredentials);
        }

        var email = request.Email.Trim();
        var check = await accounts.CheckPasswordAsync(email, request.Password, cancellationToken);

        var result = check switch
        {
            { Outcome: PasswordCheckOutcome.LockedOut } => LoginResult.Failed(LoginFailure.AccountLocked),
            { Outcome: PasswordCheckOutcome.Success, User: { IsActive: true } user } => LoginResult.Succeeded(SessionResponse.Create(user)),
            { Outcome: PasswordCheckOutcome.Success, User: not null } => LoginResult.Failed(LoginFailure.AccountDisabled),
            _ => LoginResult.Failed(LoginFailure.InvalidCredentials),
        };

        if (result.Session is { } session)
        {
            audit.Record("auth.login_succeeded", session.User.Id, userId: session.User.Id);
        }
        else
        {
            var reason = $"{result.Failure}";
            audit.Record("auth.login_failed", check.User?.Id, new { email, reason });
            metrics.LoginFailed(reason);
        }

        await db.SaveChangesAsync(cancellationToken);

        return result;
    }
}
