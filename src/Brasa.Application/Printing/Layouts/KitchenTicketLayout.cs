using Brasa.Domain.Kitchen;
using Brasa.Domain.Printing;

namespace Brasa.Application.Printing.Layouts;

public static class KitchenTicketLayout
{
    public const string CopyMark = "2ª VIA";

    public static PrintDocument Build(KitchenTicket ticket, string storeName, DateTimeOffset issuedAtStoreTime, bool isCopy)
    {
        var lines = new List<PrintLine>();

        if (isCopy)
        {
            lines.Add(PrintLine.Title(CopyMark));
        }

        lines.Add(PrintLine.Centered(storeName));
        lines.Add(PrintLine.Title($"PEDIDO #{ticket.OrderNumber}"));
        lines.Add(PrintLine.Centered(OrderTypeLabels.ForTicket(ticket.OrderType)));
        lines.Add(PrintLine.Strong(ticket.CustomerLabel.ToUpperInvariant()));
        lines.Add(PrintLine.Plain(issuedAtStoreTime.ToString("dd/MM/yyyy HH:mm")));
        lines.Add(PrintLine.Separator);

        foreach (var item in ticket.Items)
        {
            lines.Add(PrintLine.Strong($"{item.Quantity}x {item.Name.ToUpperInvariant()}"));
            lines.AddRange(item.Modifiers.Select(modifier => PrintLine.Plain($"   {modifier}")));

            if (item.Notes is { } itemNotes)
            {
                lines.Add(PrintLine.Plain($"   OBS: {itemNotes}"));
            }
        }

        if (ticket.Notes is { } orderNotes)
        {
            lines.Add(PrintLine.Separator);
            lines.Add(PrintLine.Strong($"OBS DO PEDIDO: {orderNotes}"));
        }

        lines.Add(PrintLine.Separator);
        lines.Add(PrintLine.Feed);
        lines.Add(PrintLine.Cut);

        return PrintDocument.Of(lines);
    }
}
