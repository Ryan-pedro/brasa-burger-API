using Brasa.Application.Identity.Team;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Identity;

namespace Brasa.Application.Identity;

/// <summary>Contas da equipe: conferência de senha e consulta. A implementação fica em Infrastructure.</summary>
public interface IUserAccounts
{
    /// <summary>Confere e-mail e senha, contando tentativas falhas para o bloqueio temporário.</summary>
    Task<PasswordCheckResult> CheckPasswordAsync(string email, string password, CancellationToken cancellationToken);

    Task<UserInfo?> FindAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<UserInfo>> ListAsync(CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);

    Task<Result<UserInfo>> CreateAsync(NewStaffAccount account, string password, CancellationToken cancellationToken);

    Task ChangeRoleAsync(Guid userId, StaffRole role, CancellationToken cancellationToken);

    Task SetActiveAsync(Guid userId, bool isActive, CancellationToken cancellationToken);
}
