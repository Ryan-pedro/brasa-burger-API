namespace Brasa.Application.Identity.Team;

public sealed record InvitationRequest(string? Email, string? DisplayName, string? Role);
