using Brasa.Domain.Catalog;

namespace Brasa.UnitTests.Catalog;

public class SlugsTests
{
    [Theory]
    [InlineData("Hambúrgueres", "hamburgueres")]
    [InlineData("Porções & Extras", "porcoes-extras")]
    [InlineData("  Hot   Dogs  ", "hot-dogs")]
    [InlineData("Água", "agua")]
    [InlineData("Combo 2x1!", "combo-2x1")]
    public void Gera_endereco_sem_acentos_e_com_hifens(string text, string expected)
    {
        Assert.Equal(expected, Slugs.From(text));
    }

    [Fact]
    public void Texto_sem_letras_nem_numeros_vira_vazio()
    {
        Assert.Equal(string.Empty, Slugs.From("!!! ---"));
    }
}
