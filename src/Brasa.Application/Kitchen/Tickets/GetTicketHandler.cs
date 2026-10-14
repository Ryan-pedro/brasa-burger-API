using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Kitchen.Tickets;

public sealed class GetTicketHandler(IAppDbContext db)
{
    public async Task<Result<TicketResponse>> HandleAsync(Guid id, CancellationToken cancellationToken)
    {
        var ticket = await db.KitchenTickets.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        return ticket is null ? TicketErrors.NotFound : TicketResponse.From(ticket);
    }
}
