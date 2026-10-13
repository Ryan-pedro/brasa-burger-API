using Brasa.Domain.Catalog;

namespace Brasa.UnitTests.Catalog;

public class CategoryTests
{
    [Fact]
    public void Nova_categoria_nasce_ativa_e_com_endereco_derivado_do_nome()
    {
        var category = new Category("  Sobremesas ", 3);

        Assert.Equal("Sobremesas", category.Name);
        Assert.Equal("sobremesas", category.Slug);
        Assert.Equal(3, category.SortOrder);
        Assert.True(category.IsActive);
    }

    [Fact]
    public void Renomear_atualiza_o_endereco()
    {
        var category = new Category("Porções", 0);

        category.Rename("Porções e Petiscos");

        Assert.Equal("porcoes-e-petiscos", category.Slug);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("!!!")]
    public void Nome_sem_letras_e_recusado(string name)
    {
        Assert.Throws<ArgumentException>(() => new Category(name, 0));
    }

    [Fact]
    public void Posicao_negativa_e_recusada()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Category("Bebidas", -1));
    }
}
