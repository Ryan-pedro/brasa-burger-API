using Brasa.Domain.Identity;
using Brasa.Domain.Shared;

namespace Brasa.UnitTests.Identity;

public class StaffInvitationTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 14, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Convite_guarda_so_o_hash_do_token_e_normaliza_o_email()
    {
        var (invitation, token) = StaffInvitation.Create(" Ana@Loja.dev ", "Ana", StaffRole.Kitchen, null, Now);

        Assert.Equal("ana@loja.dev", invitation.Email);
        Assert.Equal(StaffInvitation.TokenLength, token.Length);
        Assert.NotEqual(token, invitation.TokenHash);
        Assert.True(SecretHash.Matches(token, invitation.TokenHash));
    }

    [Fact]
    public void Convite_vale_ate_expirar_e_so_pode_ser_aceito_uma_vez()
    {
        var (invitation, _) = StaffInvitation.Create("ana@loja.dev", "Ana", StaffRole.Manager, null, Now);

        Assert.True(invitation.IsPendingAt(Now + StaffInvitation.Validity - TimeSpan.FromSeconds(1)));
        Assert.False(invitation.IsPendingAt(Now + StaffInvitation.Validity));

        invitation.Accept(Now);

        Assert.False(invitation.IsPendingAt(Now));
        Assert.Throws<InvalidOperationException>(() => invitation.Accept(Now));
        Assert.Throws<InvalidOperationException>(() => invitation.Revoke(Now));
    }

    [Fact]
    public void Convite_revogado_nao_pode_ser_aceito()
    {
        var (invitation, _) = StaffInvitation.Create("ana@loja.dev", "Ana", StaffRole.Manager, null, Now);

        invitation.Revoke(Now);

        Assert.Throws<InvalidOperationException>(() => invitation.Accept(Now));
    }
}
