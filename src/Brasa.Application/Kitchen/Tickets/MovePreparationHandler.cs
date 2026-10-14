using Brasa.Application.Shared;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Kitchen;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Kitchen.Tickets;

public enum PreparationMove
{
    Advance,
    Undo,
}

public sealed class MovePreparationHandler(IAppDbContext db, TimeProvider clock)
{
    public async Task<Result<TicketResponse>> HandleAsync(Guid id, PreparationMove move, PreparationMoveRequest request, CancellationToken cancellationToken)
    {
        if (!EnumText.TryParse<PreparationStatus>(request.From, out var from))
        {
            var errors = new FieldErrors();
            errors.Add("from", $"Informe a etapa atual: {EnumText.Names<PreparationStatus>()}.");
            return errors.ToError();
        }

        var ticket = await db.KitchenTickets.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (ticket is null)
        {
            return TicketErrors.NotFound;
        }

        var allowed = move == PreparationMove.Advance ? ticket.CanAdvance : ticket.CanUndo;
        if (ticket.Preparation != from || !allowed)
        {
            return TicketErrors.InvalidTransition;
        }

        var now = clock.GetUtcNow();
        if (move == PreparationMove.Advance)
        {
            ticket.Advance(now);
        }
        else
        {
            ticket.Undo(now);
        }

        await db.SaveChangesAsync(cancellationToken);

        return TicketResponse.From(ticket);
    }
}
