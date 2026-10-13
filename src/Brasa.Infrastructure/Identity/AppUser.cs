using Brasa.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace Brasa.Infrastructure.Identity;

/// <summary>Uma pessoa da equipe da loja, com o seu papel.</summary>
public sealed class AppUser : IdentityUser<Guid>
{
    public AppUser()
    {
        Id = Guid.CreateVersion7();
    }

    public string DisplayName { get; set; } = string.Empty;

    public StaffRole Role { get; set; } = StaffRole.Manager;

    /// <summary>Usuário desativado não entra, e sessões abertas caem na requisição seguinte.</summary>
    public bool IsActive { get; set; } = true;
}
