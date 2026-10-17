using Brasa.Application.Shared.Results;

namespace Brasa.Application.Kitchen.Tickets;

public static class TicketErrors
{
    public static readonly Error NotFound = Error.NotFound("ticket_not_found", "Comanda não encontrada.");

    public static readonly Error InvalidTransition = Error.Conflict("invalid_transition", "A comanda já mudou. Atualize o quadro.");
}
