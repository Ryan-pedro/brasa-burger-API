namespace Brasa.Application.Identity.Team;

public sealed record InvitationPreviewResponse(string Email, string DisplayName, string Role, string StoreName, DateTimeOffset ExpiresAt);
