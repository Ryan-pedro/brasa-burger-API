using Brasa.Application.Shared.Results;

namespace Brasa.Application.Catalog.OptionGroups;

public static class OptionGroupErrors
{
    public static readonly Error NotFound = Error.NotFound("option_group_not_found", "Grupo de opções não encontrado.");

    public static readonly Error OptionNotFound = Error.NotFound("option_not_found", "Opção não encontrada.");
}
