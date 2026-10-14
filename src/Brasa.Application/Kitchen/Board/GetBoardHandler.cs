using Brasa.Application.Kitchen.Tickets;
using Brasa.Application.Shared.Persistence;
using Brasa.Domain.Kitchen;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Kitchen.Board;

public sealed record BoardResponse(DateTimeOffset ServerTime, IReadOnlyList<TicketResponse> Tickets);

public sealed class GetBoardHandler(IAppDbContext db, TimeProvider clock)
{
    public async Task<BoardResponse> HandleAsync(CancellationToken cancellationToken)
    {
        var tickets = await db.KitchenTickets
            .AsNoTracking()
            .Where(ticket => ticket.Document == TicketDocumentStatus.Issued && ticket.Preparation != PreparationStatus.Dispatched)
            .OrderBy(ticket => ticket.IssuedAt)
            .ToListAsync(cancellationToken);

        return new BoardResponse(clock.GetUtcNow(), tickets.Select(TicketResponse.From).ToList());
    }
}
