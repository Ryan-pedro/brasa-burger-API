using Brasa.Domain.Catalog;

namespace Brasa.UnitTests.Catalog;

public class OfferTests
{
    private static readonly DateTimeOffset Start = new(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Oferta_vale_so_dentro_da_vigencia_e_quando_ativa()
    {
        var offer = Offer.ForOrderAmount("R$ 5 OFF", 30m, 5m, Start, Start.AddDays(7));

        Assert.False(offer.IsValidAt(Start.AddSeconds(-1)));
        Assert.True(offer.IsValidAt(Start));
        Assert.False(offer.IsValidAt(Start.AddDays(7)));

        offer.Deactivate();
        Assert.False(offer.IsValidAt(Start.AddDays(1)));
    }

    [Fact]
    public void Termos_precisam_corresponder_ao_tipo()
    {
        var offer = Offer.ForProduct("Smash", Guid.CreateVersion7(), 24.9m, Start, null);

        Assert.Throws<InvalidOperationException>(() => offer.SetOrderTerms(30m, 5m));
    }

    [Fact]
    public void Pedido_minimo_precisa_cobrir_o_desconto_e_fim_vem_depois_do_inicio()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Offer.ForOrderAmount("Errada", 4m, 5m, Start, null));
        Assert.Throws<ArgumentOutOfRangeException>(() => Offer.ForOrderAmount("Errada", 30m, 5m, Start, Start));
    }
}
