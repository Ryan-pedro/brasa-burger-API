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
}
