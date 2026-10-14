using Brasa.Domain.Kitchen;
using Brasa.Domain.Ordering;

namespace Brasa.UnitTests.Kitchen;

public class KitchenTicketTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 14, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Preparo_avanca_ate_despachado_e_para()
    {
        var ticket = Ticket();

        ticket.Advance(Now);
        ticket.Advance(Now);
        ticket.Advance(Now);

        Assert.Equal(PreparationStatus.Dispatched, ticket.Preparation);
        Assert.False(ticket.CanAdvance);
        Assert.False(ticket.CanUndo);
        Assert.Throws<InvalidOperationException>(() => ticket.Advance(Now));
    }

    [Fact]
    public void Desfazer_volta_um_passo_e_avisa_o_preparo()
    {
        var ticket = Ticket();
        ticket.Advance(Now);
        ticket.TakeEvents();

        ticket.Undo(Now);

        Assert.Equal(PreparationStatus.Queued, ticket.Preparation);
        Assert.Contains(ticket.TakeEvents(), domainEvent => domainEvent is PreparationChanged);
        Assert.Throws<InvalidOperationException>(() => ticket.Undo(Now));
    }

    [Fact]
    public void Comanda_anulada_nao_avanca_e_anular_de_novo_nao_tem_efeito()
    {
        var ticket = Ticket();
        ticket.Void(Now);
        ticket.TakeEvents();

        ticket.Close(Now);

        Assert.Equal(TicketDocumentStatus.Voided, ticket.Document);
        Assert.False(ticket.CanAdvance);
        Assert.Empty(ticket.TakeEvents());
    }

    private static KitchenTicket Ticket() =>
        new(Guid.CreateVersion7(), 42, OrderType.Pickup, "Ana", null, [new TicketItem(1, "X-Bacon", ["SEM CEBOLA"], null)], Now);
}
