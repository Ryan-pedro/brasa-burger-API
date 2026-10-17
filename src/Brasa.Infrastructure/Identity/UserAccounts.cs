using Brasa.Application.Identity;
using Brasa.Application.Identity.Team;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

    public async Task<IReadOnlyList<UserInfo>> ListAsync(CancellationToken cancellationToken)
    {
        var users = await userManager.Users
            .AsNoTracking()
            .OrderBy(user => user.Role)
            .ThenBy(user => user.DisplayName)
            .ToListAsync(cancellationToken);

        return users.Select(ToInfo).ToList();
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) =>
        await userManager.FindByEmailAsync(email) is not null;

    public async Task<Result<UserInfo>> CreateAsync(NewStaffAccount account, string password, CancellationToken cancellationToken)
    {
        if (await EmailExistsAsync(account.Email, cancellationToken))
        {
            return TeamErrors.MemberExists;
        }

        var user = new AppUser
        {
            UserName = account.Email,
            Email = account.Email,
            EmailConfirmed = true,
            DisplayName = account.DisplayName,
            Role = account.Role,
        };

        var passwordErrors = new FieldErrors();
        foreach (var validator in userManager.PasswordValidators)
        {
            var validation = await validator.ValidateAsync(userManager, user, password);
            foreach (var error in validation.Errors)
            {
                passwordErrors.Add("password", PasswordMessages.For(error));
            }
        }

        if (passwordErrors.Any)
        {
            return passwordErrors.ToError();
        }

        var created = await userManager.CreateAsync(user, password);
        if (!created.Succeeded)
        {
            throw new InvalidOperationException($"Falha ao criar a conta: {string.Join("; ", created.Errors.Select(error => error.Code))}");
        }

        return ToInfo(user);
    }

    public async Task ChangeRoleAsync(Guid userId, StaffRole role, CancellationToken cancellationToken)
    {
        var user = await RequireAsync(userId);
        user.Role = role;
        await SaveWithNewSecurityStampAsync(user);
    }

    public async Task SetActiveAsync(Guid userId, bool isActive, CancellationToken cancellationToken)
    {
        var user = await RequireAsync(userId);
        user.IsActive = isActive;
        await SaveWithNewSecurityStampAsync(user);
    }

    private async Task<AppUser> RequireAsync(Guid userId) =>
        await userManager.FindByIdAsync(userId.ToString())
        ?? throw new InvalidOperationException("Usuário não encontrado.");

    private async Task SaveWithNewSecurityStampAsync(AppUser user)
    {
        var saved = await userManager.UpdateSecurityStampAsync(user);
        if (!saved.Succeeded)
        {
            throw new InvalidOperationException($"Falha ao salvar a conta: {string.Join("; ", saved.Errors.Select(error => error.Code))}");
        }
    }

    private static UserInfo ToInfo(AppUser user) =>
        new(user.Id, user.Email ?? string.Empty, user.DisplayName, user.Role, user.IsActive);
}
