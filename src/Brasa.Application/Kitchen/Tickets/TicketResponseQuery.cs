using Brasa.Application.Printing.Jobs;
using Brasa.Application.Shared.Persistence;
using Brasa.Domain.Kitchen;

namespace Brasa.Application.Kitchen.Tickets;

public static class TicketResponseQuery
{
    public static async Task<IReadOnlyList<TicketResponse>> ToResponsesAsync(
        this IAppDbContext db,
        IReadOnlyList<KitchenTicket> tickets,
        CancellationToken cancellationToken)
    {
        var printStatuses = await db.LatestPrintStatusByTicketAsync(tickets.Select(ticket => ticket.Id).ToList(), cancellationToken);
        return tickets.Select(ticket => TicketResponse.From(ticket, printStatuses.GetValueOrDefault(ticket.Id))).ToList();
    }

    public static async Task<TicketResponse> ToResponseAsync(this IAppDbContext db, KitchenTicket ticket, CancellationToken cancellationToken) =>
        (await db.ToResponsesAsync([ticket], cancellationToken))[0];
}
