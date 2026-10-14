using Brasa.Domain.Catalog;

namespace Brasa.UnitTests.Catalog;

public class OptionGroupTests
{
    [Fact]
    public void Editar_as_opcoes_preserva_o_id_das_que_continuam()
    {
        var group = new OptionGroup("Bebida", OptionKind.Choice, 1, 1);
        group.SetOptions([new OptionDraft(null, "Coca", 0m), new OptionDraft(null, "Guaraná", 0m)]);
        var coca = group.Options[0];

        group.SetOptions([new OptionDraft(null, "Suco", 3.9m), new OptionDraft(coca.Id, "Coca-Cola", 0m)]);

        Assert.Equal(["Suco", "Coca-Cola"], group.Options.Select(option => option.Name));
        Assert.Equal(coca.Id, group.Options[1].Id);
    }

    [Fact]
    public void Opcoes_com_nome_repetido_sao_recusadas()
    {
        var group = new OptionGroup("Adicionais", OptionKind.Addon, 0, 3);

        Assert.Throws<ArgumentException>(() => group.SetOptions([new OptionDraft(null, "Bacon", 5m), new OptionDraft(null, " bacon ", 5m)]));
    }

    [Fact]
    public void Remocao_nao_tem_preco_nem_pode_ser_obrigatoria()
    {
        var group = new OptionGroup("Retirar", OptionKind.Removal, 0, 2);

        Assert.Throws<ArgumentException>(() => group.SetOptions([new OptionDraft(null, "Cebola", 1m)]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new OptionGroup("Retirar", OptionKind.Removal, 1, 2));
    }

    [Fact]
    public void Minimo_maior_que_o_maximo_e_recusado()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new OptionGroup("Tamanho", OptionKind.Choice, 2, 1));
    }

    [Fact]
    public void Escolha_obrigatoria_sem_opcao_disponivel_nao_pode_ser_atendida()
    {
        var group = new OptionGroup("Bebida", OptionKind.Choice, 1, 1);
        group.SetOptions([new OptionDraft(null, "Coca", 0m)]);

        group.SetOptionAvailability(group.Options[0].Id, false);

        Assert.False(group.CanBeFulfilled);
    }
}
