using Brasa.Domain.Identity;

namespace Brasa.Application.Identity.Team;

public sealed record NewStaffAccount(string Email, string DisplayName, StaffRole Role);
