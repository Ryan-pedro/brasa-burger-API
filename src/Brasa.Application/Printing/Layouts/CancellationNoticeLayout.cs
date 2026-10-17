using Brasa.Domain.Kitchen;
using Brasa.Domain.Printing;

namespace Brasa.Application.Printing.Layouts;

public static class CancellationNoticeLayout
{
    public static PrintDocument Build(KitchenTicket ticket, string storeName, DateTimeOffset cancelledAtStoreTime) =>
        PrintDocument.Of(
        [
            PrintLine.Centered(storeName),
            PrintLine.Title("*** CANCELADO ***"),
            PrintLine.Title($"PEDIDO #{ticket.OrderNumber}"),
            PrintLine.Strong(ticket.CustomerLabel.ToUpperInvariant()),
            PrintLine.Plain(cancelledAtStoreTime.ToString("dd/MM/yyyy HH:mm")),
            PrintLine.Separator,
            PrintLine.Plain("Não prepare este pedido."),
            PrintLine.Feed,
            PrintLine.Cut,
        ]);
}
