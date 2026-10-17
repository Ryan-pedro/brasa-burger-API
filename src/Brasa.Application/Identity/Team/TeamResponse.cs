namespace Brasa.Application.Identity.Team;

public sealed record TeamResponse(IReadOnlyList<TeamMemberResponse> Members, IReadOnlyList<PendingInvitationResponse> Invitations);
