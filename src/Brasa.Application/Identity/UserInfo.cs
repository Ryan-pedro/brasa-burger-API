using Brasa.Domain.Identity;

namespace Brasa.Application.Identity;

public sealed record UserInfo(Guid Id, string Email, string DisplayName, StaffRole Role, bool IsActive);
