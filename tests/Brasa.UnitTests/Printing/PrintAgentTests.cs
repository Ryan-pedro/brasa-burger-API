using Brasa.Domain.Printing;

namespace Brasa.UnitTests.Printing;

public class PrintAgentTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 14, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Pareamento_gera_segredo_que_so_confere_com_o_proprio_agente()
    {
        var (agent, secret) = PrintAgent.Pair("Caixa", "1.0.0", Now);

        Assert.True(agent.HasSecret(secret));
        Assert.False(agent.HasSecret(secret + "x"));
        Assert.NotEqual(secret, agent.SecretHash);
    }

    [Fact]
    public void Agente_fica_offline_sem_sinal_de_vida_e_volta_no_proximo_sinal()
    {
        var (agent, _) = PrintAgent.Pair("Caixa", "1.0.0", Now);
        agent.TakeEvents();

        Assert.False(agent.IsSilentSince(Now.AddSeconds(89)));
        Assert.True(agent.IsSilentSince(Now + PrintAgent.OfflineAfter));

        agent.MarkOffline();
        agent.RecordHeartbeat("1.0.1", 0, null, Now.AddMinutes(5));

        Assert.True(agent.IsOnline);
        Assert.Equal("1.0.1", agent.AgentVersion);
        Assert.Equal(2, agent.TakeEvents().Count);
    }

    [Fact]
    public void Agente_revogado_perde_o_segredo_e_nao_recebe_sinal()
    {
        var (agent, secret) = PrintAgent.Pair("Caixa", "1.0.0", Now);

        agent.Revoke(Now);

        Assert.False(agent.HasSecret(secret));
        Assert.Throws<InvalidOperationException>(() => agent.RecordHeartbeat("1.0.0", 0, null, Now));
    }

    [Fact]
    public void Codigo_de_pareamento_vale_uma_vez_e_por_tempo_limitado()
    {
        var (code, plain) = PairingCode.Generate(null, Now);

        Assert.Equal(PairingCode.CodeLength, plain.Length);
        Assert.Equal(plain, PairingCode.Normalize($"{plain[..4].ToLowerInvariant()}-{plain[4..]}"));
        Assert.False(code.IsUsableAt(Now + PairingCode.Validity));

        code.MarkUsed(Now);
        Assert.Throws<InvalidOperationException>(() => code.MarkUsed(Now));
    }

    [Fact]
    public void Impressora_inativa_nao_pode_ser_padrao()
    {
        var printer = new Printer(Guid.CreateVersion7(), "Cozinha", PaperWidth.Mm58, "tcp://192.168.0.50:9100");
        printer.MakeDefault();

        printer.Deactivate();

        Assert.False(printer.IsDefault);
        Assert.Equal(32, printer.Columns);
        Assert.Throws<InvalidOperationException>(() => printer.MakeDefault());
    }
}
