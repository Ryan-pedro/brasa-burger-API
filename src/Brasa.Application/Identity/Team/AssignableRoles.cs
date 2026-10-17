using Brasa.Application.Shared;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Identity;

namespace Brasa.Application.Identity.Team;

internal static class AssignableRoles
{
    private static readonly StaffRole[] Roles = [StaffRole.Manager, StaffRole.Kitchen];

    public static StaffRole? Parse(string? text, FieldErrors errors)
    {
        if (EnumText.TryParse<StaffRole>(text, out var role) && Roles.Contains(role))
        {
            return role;
        }

        errors.Add("role", $"Escolha o papel: {string.Join(", ", Roles)}.");
        return null;
    }
}
