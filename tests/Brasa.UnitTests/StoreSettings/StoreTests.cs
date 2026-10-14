using Brasa.Domain.Ordering;
using Brasa.Domain.StoreSettings;

namespace Brasa.UnitTests.StoreSettings;

public class StoreTests
{
    [Fact]
    public void A_loja_sempre_usa_o_identificador_fixo()
    {
        var store = new Store("Brasa Burger", "America/Sao_Paulo");

        Assert.Equal(Store.SingletonId, store.Id);
    }

    [Fact]
    public void Nome_e_gravado_sem_espacos_nas_pontas()
    {
        var store = new Store("  Brasa Burger  ", "America/Sao_Paulo");

        Assert.Equal("Brasa Burger", store.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Nome_vazio_e_recusado(string name)
    {
        Assert.Throws<ArgumentException>(() => new Store(name, "America/Sao_Paulo"));
    }

    [Fact]
    public void Nome_longo_demais_e_recusado()
    {
        var name = new string('a', Store.NameMaxLength + 1);

        Assert.Throws<ArgumentException>(() => new Store(name, "America/Sao_Paulo"));
    }

    [Fact]
    public void Loja_nova_abre_aceitando_todos_os_tipos_e_com_aceite_automatico()
    {
        var store = new Store("Brasa", "America/Sao_Paulo");

        Assert.True(store.IsOpen);
        Assert.True(store.Accepts(OrderType.DineIn) && store.Accepts(OrderType.Pickup) && store.Accepts(OrderType.Delivery));
        Assert.False(store.ManualAcceptance);
    }

    [Fact]
    public void Loja_precisa_aceitar_ao_menos_um_tipo_de_pedido()
    {
        var store = new Store("Brasa", "America/Sao_Paulo");

        Assert.Throws<ArgumentException>(() => store.SetOrderTypes(false, false, false));
    }

    [Fact]
    public void Taxa_de_entrega_so_vale_para_delivery()
    {
        var store = new Store("Brasa", "America/Sao_Paulo");
        store.SetDeliveryFee(6.9m);

        Assert.Equal(6.9m, store.DeliveryFeeFor(OrderType.Delivery));
        Assert.Equal(0m, store.DeliveryFeeFor(OrderType.Pickup));
        Assert.Throws<ArgumentOutOfRangeException>(() => store.SetDeliveryFee(-1m));
    }
}

