using Brasa.Application.Identity;
using Brasa.Domain.Identity;

namespace Brasa.UnitTests.Identity;

public class PermissionsTests
{
    [Fact]
    public void Cozinha_nao_tem_nenhuma_permissao_administrativa()
    {
        var kitchen = Permissions.For(StaffRole.Kitchen);

        Assert.All(kitchen, permission => Assert.StartsWith("kitchen.", permission));
    }

    [Theory]
    [InlineData(StaffRole.Owner)]
    [InlineData(StaffRole.Manager)]
    public void Papeis_administrativos_nao_herdam_as_permissoes_da_cozinha(StaffRole role)
    {
        var permissions = Permissions.For(role);

        Assert.DoesNotContain(permissions, permission => permission.StartsWith("kitchen.", StringComparison.Ordinal));
    }

    [Fact]
    public void Gerente_nao_gerencia_a_equipe()
    {
        Assert.DoesNotContain(Permissions.TeamManage, Permissions.For(StaffRole.Manager));
    }

    [Fact]
    public void Somente_o_proprietario_atende_pedidos_de_titular()
    {
        Assert.Contains(Permissions.PrivacyManage, Permissions.For(StaffRole.Owner));
        Assert.DoesNotContain(Permissions.PrivacyManage, Permissions.For(StaffRole.Manager));
        Assert.DoesNotContain(Permissions.PrivacyManage, Permissions.For(StaffRole.Kitchen));
    }

    [Fact]
    public void Proprietario_tem_tudo_do_gerente_e_mais_a_equipe()
    {
        var owner = Permissions.For(StaffRole.Owner);

        Assert.All(Permissions.For(StaffRole.Manager), permission => Assert.Contains(permission, owner));
        Assert.Contains(Permissions.TeamManage, owner);
    }
}
