using Brasa.Domain.Catalog;
using Brasa.Domain.Ordering;

namespace Brasa.UnitTests.Ordering;

public class CartPricingTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 14, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid Burger = Guid.CreateVersion7();
    private static readonly Guid Fries = Guid.CreateVersion7();
    private static readonly Guid Soda = Guid.CreateVersion7();

    private static readonly Offer FiveOff = Offer.ForOrderAmount("R$ 5 OFF no app", 30m, 5m, Now.AddDays(-1), null);

    [Fact]
    public void Soma_adicionais_aplica_r5_off_a_partir_de_r30_e_taxa_so_no_delivery()
    {
        IReadOnlyList<CartLine> lines =
        [
            Line(Burger, 24.9m, 2, Addon("Bacon", 5m), Addon("Queijo", 3m)),
            Line(Fries, 19.9m, 1),
            Line(Soda, 6m, 2),
        ];

        var pickup = CartPricing.Calculate(lines, [FiveOff], 0m, Now);
        var delivery = CartPricing.Calculate(lines, [FiveOff], 6.9m, Now);

        Assert.Equal(97.7m, pickup.Subtotal);
        Assert.Equal(5m, pickup.DiscountAmount);
        Assert.Equal(92.7m, pickup.Total);
        Assert.Equal(99.6m, delivery.Total);
    }

    [Fact]
    public void Sem_desconto_abaixo_do_minimo()
    {
        var price = CartPricing.Calculate([Line(Soda, 6m, 1)], [FiveOff], 0m, Now);

        Assert.Null(price.Discount);
        Assert.Equal(6m, price.Total);
    }

    [Fact]
    public void Vale_o_maior_desconto_entre_as_ofertas()
    {
        var promo = Offer.ForProduct("Burger por 14,90", Burger, 14.9m, Now.AddDays(-1), null);

        var price = CartPricing.Calculate([Line(Burger, 24.9m, 2)], [FiveOff, promo], 0m, Now);

        Assert.Equal("Burger por 14,90", price.Discount!.Label);
        Assert.Equal(20m, price.DiscountAmount);
        Assert.Equal(29.8m, price.Total);
    }

    [Fact]
    public void Oferta_fora_da_vigencia_nao_conta()
    {
        var expired = Offer.ForOrderAmount("Antiga", 10m, 9m, Now.AddDays(-10), Now.AddDays(-1));

        var price = CartPricing.Calculate([Line(Burger, 24.9m, 1)], [expired], 0m, Now);

        Assert.Null(price.Discount);
    }

    [Fact]
    public void Diferenca_da_bebida_do_combo_entra_no_preco_unitario()
    {
        var line = Line(Burger, 32.9m, 1, new PricedOption(Guid.CreateVersion7(), "Bebida", "Suco de laranja", OptionKind.Choice, 3.9m));

        Assert.Equal(36.8m, line.UnitPrice);
    }

    private static CartLine Line(Guid productId, decimal price, int quantity, params PricedOption[] options) =>
        new(productId, "Produto", price, quantity, options, null);

    private static PricedOption Addon(string name, decimal price) =>
        new(Guid.CreateVersion7(), "Adicionais", name, OptionKind.Addon, price);
}
