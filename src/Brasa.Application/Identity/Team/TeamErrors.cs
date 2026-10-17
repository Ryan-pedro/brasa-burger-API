using Brasa.Application.Shared.Results;

namespace Brasa.Application.Identity.Team;

public static class TeamErrors
{
    public static readonly Error InvitationNotFound = Error.NotFound("invitation_not_found", "Convite não encontrado, já usado, revogado ou expirado.");

    public static readonly Error MemberNotFound = Error.NotFound("team_member_not_found", "Pessoa da equipe não encontrada.");

    public static readonly Error MemberExists = Error.Conflict("team_member_exists", "Já existe uma pessoa da equipe com este e-mail.");

    public static readonly Error CannotChangeOwnAccount = Error.Rule("cannot_change_own_account", "Você não pode alterar o papel nem desativar a sua própria conta.");

    public static readonly Error OwnerAccountProtected = Error.Rule("owner_account_protected", "A conta do proprietário não pode ser alterada pelo painel.");
}
