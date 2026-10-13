using Brasa.Application.Identity;
using Microsoft.AspNetCore.Identity;

namespace Brasa.Infrastructure.Identity;

internal sealed class UserAccounts(UserManager<AppUser> userManager) : IUserAccounts
{
    public async Task<PasswordCheckResult> CheckPasswordAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return new PasswordCheckResult(PasswordCheckOutcome.InvalidCredentials, null);
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return new PasswordCheckResult(PasswordCheckOutcome.LockedOut, null);
        }

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            // Conta a falha. Ao atingir o limite, a conta fica bloqueada por um tempo.
            await userManager.AccessFailedAsync(user);

            var outcome = await userManager.IsLockedOutAsync(user)
                ? PasswordCheckOutcome.LockedOut
                : PasswordCheckOutcome.InvalidCredentials;

            return new PasswordCheckResult(outcome, null);
        }

        await userManager.ResetAccessFailedCountAsync(user);

        return new PasswordCheckResult(PasswordCheckOutcome.Success, ToInfo(user));
    }

    public async Task<UserInfo?> FindAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user is null ? null : ToInfo(user);
    }

    private static UserInfo ToInfo(AppUser user) =>
        new(user.Id, user.Email ?? string.Empty, user.DisplayName, user.Role, user.IsActive);
}
