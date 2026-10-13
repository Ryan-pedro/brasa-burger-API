using Brasa.Domain.Identity;

namespace Brasa.Application.Shared.Auth;

/// <summary>
/// Quem está fazendo a requisição autenticada e com qual papel.
/// É preenchido uma única vez, pelo servidor, depois de reler o usuário no banco.
/// </summary>
public sealed class CurrentUser
{
    public Guid? UserId { get; private set; }

    public StaffRole? Role { get; private set; }

    public bool IsAuthenticated => UserId is not null;

    public void Set(Guid userId, StaffRole role)
    {
        if (UserId is not null)
        {
            throw new InvalidOperationException("O usuário corrente já foi definido e não pode ser alterado.");
        }

        UserId = userId;
        Role = role;
    }

    public Guid RequireUserId() =>
        UserId ?? throw new InvalidOperationException("Esta operação exige um usuário autenticado.");
}
