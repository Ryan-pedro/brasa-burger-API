using Brasa.Domain.Catalog;
using Brasa.Domain.Ordering;
using Brasa.Domain.Payments;

namespace Brasa.UnitTests.Ordering;

public class OrderTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 14, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Pedido_nasce_recebido_com_copia_de_nome_e_preco_e_avisa_a_criacao()
    {
        var order = Pickup();

        Assert.Equal(OrderStatus.Placed, order.Status);
        Assert.Equal("X-Bacon", Assert.Single(order.Items).ProductName);
        Assert.Equal(29.9m, order.Total);
        Assert.Equal(Order.TrackingTokenLength, order.TrackingToken.Length);
        Assert.IsType<OrderPlaced>(Assert.Single(order.TakeEvents()));
    }

    [Fact]
    public void Transicoes_seguem_a_maquina_de_estados()
    {
        var order = Pickup();

        order.Confirm(Now);
        order.FollowPreparation(OrderStatus.InPreparation, Now);
        order.FollowPreparation(OrderStatus.Ready, Now);
        order.Dispatch(Now);

        Assert.Equal(OrderStatus.Completed, order.Status);
        Assert.Throws<InvalidOperationException>(() => order.Cancel("tarde demais", Now));
    }

    [Fact]
    public void Delivery_despachado_sai_para_entrega_antes_de_finalizar()
    {
        var order = new Order(
            1,
            OrderType.Delivery,
            new OrderCustomer("Ana", "31999990000", null),
            new DeliveryAddress("Rua A", "10", null, "Centro"),
            null,
            PaymentMethod.Card,
            null,
            [Line()],
            new PriceBreakdown(29.9m, null, 6.9m),
            Now);

        order.Confirm(Now);
        order.FollowPreparation(OrderStatus.InPreparation, Now);
        order.FollowPreparation(OrderStatus.Ready, Now);
        order.Dispatch(Now);

        Assert.Equal(OrderStatus.OutForDelivery, order.Status);
        Assert.Equal(36.8m, order.Total);
    }

    [Fact]
    public void Recusa_so_vale_para_pedido_ainda_nao_aceito_e_exige_motivo()
    {
        var order = Pickup();

        Assert.Throws<ArgumentException>(() => order.Reject("  ", Now));
        order.Confirm(Now);
        Assert.Throws<InvalidOperationException>(() => order.Reject("sem insumo", Now));
    }

    [Fact]
    public void Dados_do_cliente_precisam_corresponder_ao_tipo()
    {
        Assert.Throws<ArgumentException>(() => new Order(
            1, OrderType.DineIn, new OrderCustomer("Ana", null, null), null, null, PaymentMethod.Pix, null, [Line()], new PriceBreakdown(29.9m, null, 0m), Now));
        Assert.Throws<ArgumentException>(() => new Order(
            1, OrderType.Delivery, new OrderCustomer("Ana", "31999990000", null), null, null, PaymentMethod.Pix, null, [Line()], new PriceBreakdown(29.9m, null, 0m), Now));
    }

    [Fact]
    public void Troco_so_em_dinheiro_e_cobrindo_o_total()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Order(
            1, OrderType.Pickup, new OrderCustomer("Ana", null, null), null, null, PaymentMethod.Cash, 20m, [Line()], new PriceBreakdown(29.9m, null, 0m), Now));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Order(
            1, OrderType.Pickup, new OrderCustomer("Ana", null, null), null, null, PaymentMethod.Pix, 50m, [Line()], new PriceBreakdown(29.9m, null, 0m), Now));
    }

    [Fact]
    public void Texto_livre_perde_caracteres_de_controle()
    {
        var order = new Order(
            1, OrderType.DineIn, new OrderCustomer(null, null, " 12\u001b "), null, "sem\u0007 gelo", PaymentMethod.Pix, null, [Line()], new PriceBreakdown(29.9m, null, 0m), Now);

        Assert.Equal("12", order.Customer.Table);
        Assert.Equal("sem gelo", order.Notes);
    }

    private static Order Pickup() =>
        new(1, OrderType.Pickup, new OrderCustomer("Ana", null, null), null, null, PaymentMethod.Pix, null, [Line()], new PriceBreakdown(29.9m, null, 0m), Now);

    private static CartLine Line() =>
        new(Guid.CreateVersion7(), "X-Bacon", 24.9m, 1, [new PricedOption(Guid.CreateVersion7(), "Adicionais", "Bacon", OptionKind.Addon, 5m)], null);
}
