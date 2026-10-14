using Brasa.Domain.Catalog;

namespace Brasa.UnitTests.Catalog;

public class ProductTests
{
    private static readonly Guid CategoryId = Guid.CreateVersion7();

    [Fact]
    public void Novo_produto_nasce_disponivel_e_sem_destaque()
    {
        var product = new Product(CategoryId, " X-Bacon ", " Pão brioche ", 24.90m, null, null, 0);

        Assert.Equal("X-Bacon", product.Name);
        Assert.Equal("Pão brioche", product.Description);
        Assert.True(product.IsAvailable);
        Assert.False(product.IsFeatured);
    }

    [Fact]
    public void Descricao_nula_vira_vazia_e_campos_opcionais_vazios_viram_nulos()
    {
        var product = new Product(CategoryId, "Água", null, 4m, "  ", "", 0);

        Assert.Equal(string.Empty, product.Description);
        Assert.Null(product.Size);
        Assert.Null(product.ImageUrl);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("10000")]
    [InlineData("12.345")]
    public void Preco_invalido_e_recusado(string price)
    {
        var value = decimal.Parse(price, System.Globalization.CultureInfo.InvariantCulture);

        Assert.Throws<ArgumentOutOfRangeException>(() => new Product(CategoryId, "X-Bacon", null, value, null, null, 0));
    }

    [Fact]
    public void Produto_sem_categoria_e_recusado()
    {
        Assert.Throws<ArgumentException>(() => new Product(Guid.Empty, "X-Bacon", null, 10m, null, null, 0));
    }

    [Fact]
    public void Disponibilidade_e_destaque_podem_ser_alterados()
    {
        var product = new Product(CategoryId, "X-Bacon", null, 24.90m, null, null, 0);

        product.SetAvailability(false);
        product.SetFeatured(true);

        Assert.False(product.IsAvailable);
        Assert.True(product.IsFeatured);
    }

    [Fact]
    public void Grupos_de_opcoes_seguem_a_ordem_informada()
    {
        var product = new Product(CategoryId, "X-Bacon", null, 24.90m, null, null, 0);
        var extras = Guid.CreateVersion7();
        var removal = Guid.CreateVersion7();

        product.SetOptionGroups([extras, removal]);
        product.SetOptionGroups([removal, extras]);

        Assert.Equal([removal, extras], product.OptionGroupIds);
    }

    [Fact]
    public void Grupo_de_opcoes_repetido_e_recusado()
    {
        var product = new Product(CategoryId, "X-Bacon", null, 24.90m, null, null, 0);
        var group = Guid.CreateVersion7();

        Assert.Throws<ArgumentException>(() => product.SetOptionGroups([group, group]));
    }
}
