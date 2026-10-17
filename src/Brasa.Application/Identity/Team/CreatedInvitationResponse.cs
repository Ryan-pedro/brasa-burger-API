namespace Brasa.Application.Identity.Team;

public sealed record CreatedInvitationResponse(Guid Id, string Email, string DisplayName, string Role, string Token, DateTimeOffset ExpiresAt);
