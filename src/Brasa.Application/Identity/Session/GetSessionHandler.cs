using Brasa.Application.Shared.Auth;

namespace Brasa.Application.Identity.Session;

public sealed class GetSessionHandler(CurrentUser currentUser, IUserAccounts accounts)
{
    public async Task<SessionResponse?> HandleAsync(CancellationToken cancellationToken)
    {
        var user = await accounts.FindAsync(currentUser.RequireUserId(), cancellationToken);

        return user is { IsActive: true } ? SessionResponse.Create(user) : null;
    }
}
