using Brasa.Application.Kitchen.Tickets;
using Brasa.Application.Shared.Persistence;
using Brasa.Domain.Kitchen;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Kitchen.Board;

public sealed class GetBoardHandler(IAppDbContext db, TimeProvider clock)
{
    public async Task<BoardResponse> HandleAsync(CancellationToken cancellationToken)
    {
        var tickets = await db.KitchenTickets
            .AsNoTracking()
            .Where(ticket => ticket.Document == TicketDocumentStatus.Issued && ticket.Preparation != PreparationStatus.Dispatched)
            .OrderBy(ticket => ticket.IssuedAt)
            .ToListAsync(cancellationToken);

        var printers = await db.Printers
            .AsNoTracking()
            .Where(printer => printer.IsActive)
            .Join(db.PrintAgents, printer => printer.AgentId, agent => agent.Id, (printer, agent) => new { Printer = printer, agent.IsOnline })
            .ToListAsync(cancellationToken);

        var boardPrinters = printers
            .Select(row => new BoardPrinter(row.Printer.Id, row.Printer.Name, row.Printer.IsDefault, row.IsOnline, row.Printer.Status.ToString(), row.Printer.StatusMessage))
            .ToList();

        return new BoardResponse(clock.GetUtcNow(), await db.ToResponsesAsync(tickets, cancellationToken), boardPrinters);
    }
}
