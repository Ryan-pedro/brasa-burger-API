using Brasa.Domain.Identity;

namespace Brasa.Application.Identity;

/// <summary>Contas da equipe: conferência de senha e consulta. A implementação fica em Infrastructure.</summary>
public interface IUserAccounts
{
    /// <summary>Confere e-mail e senha, contando tentativas falhas para o bloqueio temporário.</summary>
    Task<PasswordCheckResult> CheckPasswordAsync(string email, string password, CancellationToken cancellationToken);

    Task<UserInfo?> FindAsync(Guid userId, CancellationToken cancellationToken);
}

public sealed record UserInfo(Guid Id, string Email, string DisplayName, StaffRole Role, bool IsActive);

public enum PasswordCheckOutcome
{
    Success,
    InvalidCredentials,
    LockedOut,
}

public sealed record PasswordCheckResult(PasswordCheckOutcome Outcome, UserInfo? User);
