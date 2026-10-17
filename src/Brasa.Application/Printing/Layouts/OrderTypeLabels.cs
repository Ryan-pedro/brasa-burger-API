using Brasa.Domain.Ordering;

namespace Brasa.Application.Printing.Layouts;

public static class OrderTypeLabels
{
    public static string ForTicket(OrderType type) => type switch
    {
        OrderType.DineIn => "CONSUMO NO LOCAL",
        OrderType.Pickup => "RETIRADA NO BALCÃO",
        _ => "ENTREGA",
    };
}
