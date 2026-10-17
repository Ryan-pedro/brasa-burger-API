using Brasa.Application.Printing.Layouts;
using Brasa.Domain.Kitchen;
using Brasa.Domain.Ordering;
using Brasa.Domain.Printing;

namespace Brasa.UnitTests.Printing;

public class KitchenTicketLayoutTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 14, 12, 30, 0, TimeSpan.Zero);

    [Fact]
    public void Comanda_traz_numero_tipo_itens_opcoes_e_observacoes()
    {
        var ticket = new KitchenTicket(Guid.CreateVersion7(), 42, OrderType.DineIn, "Mesa 12", "sem pressa", [new TicketItem(2, "X-Bacon", ["SEM CEBOLA", "+ OVO"], "bem passado")], Now);

        var texts = KitchenTicketLayout.Build(ticket, "Brasa Burger", Now, isCopy: false).Lines.Select(line => line.Text).ToList();

        Assert.Contains("PEDIDO #42", texts);
        Assert.Contains("CONSUMO NO LOCAL", texts);
        Assert.Contains("2x X-BACON", texts);
        Assert.Contains("   SEM CEBOLA", texts);
        Assert.Contains("   OBS: bem passado", texts);
        Assert.Contains("OBS DO PEDIDO: sem pressa", texts);
        Assert.DoesNotContain(KitchenTicketLayout.CopyMark, texts);
    }

    [Fact]
    public void Reimpressao_comeca_pela_marca_de_segunda_via_e_termina_com_corte()
    {
        var ticket = new KitchenTicket(Guid.CreateVersion7(), 7, OrderType.Pickup, "Ana", null, [new TicketItem(1, "Batata", [], null)], Now);

        var lines = KitchenTicketLayout.Build(ticket, "Brasa Burger", Now, isCopy: true).Lines;

        Assert.Equal(KitchenTicketLayout.CopyMark, lines[0].Text);
        Assert.Equal(PrintLineKind.Cut, lines[^1].Kind);
    }
}
