using Brasa.Domain.Printing;

namespace Brasa.UnitTests.Printing;

public class PrintJobTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 14, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid AgentId = Guid.CreateVersion7();

    [Fact]
    public void Reserva_cria_tentativa_e_confirmacao_repetida_nao_muda_nada()
    {
        var job = AutomaticJob();

        var attempt = job.Lease(AgentId, Now);
        job.ReportPrinted(attempt.Id, Now);
        job.ReportPrinted(attempt.Id, Now.AddSeconds(5));

        Assert.Equal(PrintJobStatus.Printed, job.Status);
        Assert.Equal(PrintAttemptOutcome.Printed, Assert.Single(job.Attempts).Outcome);
        Assert.Equal(Now, job.CompletedAt);
    }

    [Fact]
    public void Falha_volta_para_a_fila_com_espera_crescente()
    {
        var job = AutomaticJob();

        job.ReportFailed(job.Lease(AgentId, Now).Id, "Sem papel", Now);

        Assert.Equal(PrintJobStatus.Pending, job.Status);
        Assert.Equal("Sem papel", job.LastError);
        Assert.False(job.CanBeLeasedAt(Now.AddSeconds(4)));
        Assert.True(job.CanBeLeasedAt(Now.AddSeconds(5)));
    }

    [Fact]
    public void Tentativas_esgotadas_abandonam_a_tarefa()
    {
        var job = new PrintJob(Guid.CreateVersion7(), Guid.CreateVersion7(), PrintJobReason.Reprint, Document(), Now);
        var moment = Now;

        for (var attempt = 0; attempt < PrintJobPolicy.MaxAttempts; attempt++)
        {
            job.ReportFailed(job.Lease(AgentId, moment).Id, "Desligada", moment);
            moment = job.NextAttemptAt;
        }

        Assert.Equal(PrintJobStatus.Abandoned, job.Status);
        Assert.Equal(PrintJobPolicy.MaxAttempts, job.Attempts.Count);
    }

    [Fact]
    public void Reserva_vencida_e_resultado_desconhecido_marcam_possivel_segunda_via()
    {
        var job = AutomaticJob();

        job.Lease(AgentId, Now);
        Assert.False(job.ExpireLease(Now.AddSeconds(30)));
        Assert.True(job.ExpireLease(Now + PrintJobPolicy.LeaseDuration));
        Assert.Equal(PrintAttemptOutcome.LeaseExpired, job.Attempts[0].Outcome);

        var second = job.Lease(AgentId, job.NextAttemptAt);
        job.ReportUnknownOutcome(second.Id, job.NextAttemptAt);

        Assert.True(job.PossibleDuplicate);
        Assert.Equal(PrintJobStatus.Pending, job.Status);
    }

    [Fact]
    public void Confirmacao_tardia_de_reserva_vencida_ainda_conta_como_impressa()
    {
        var job = AutomaticJob();
        var attempt = job.Lease(AgentId, Now);
        job.ExpireLease(Now + PrintJobPolicy.LeaseDuration);

        job.ReportPrinted(attempt.Id, Now.AddMinutes(2));

        Assert.Equal(PrintJobStatus.Printed, job.Status);
    }

    [Fact]
    public void Tarefa_automatica_vencida_e_abandonada_sem_imprimir()
    {
        var job = AutomaticJob();
        var afterValidity = Now + PrintJobPolicy.AutomaticValidity;

        Assert.False(job.CanBeLeasedAt(afterValidity));
        Assert.True(job.AbandonIfExpired(afterValidity));
        Assert.Equal(PrintJobStatus.Abandoned, job.Status);
    }

    [Fact]
    public void Reimpressao_nao_vence_e_teste_nao_precisa_de_comanda()
    {
        var reprint = new PrintJob(Guid.CreateVersion7(), Guid.CreateVersion7(), PrintJobReason.Reprint, Document(), Now);
        var test = new PrintJob(Guid.CreateVersion7(), null, PrintJobReason.Test, Document(), Now);

        Assert.Null(reprint.ExpiresAt);
        Assert.Null(test.TicketId);
        Assert.Throws<ArgumentException>(() => new PrintJob(Guid.CreateVersion7(), null, PrintJobReason.Automatic, Document(), Now));
    }

    [Fact]
    public void Cancelar_durante_a_reserva_indica_que_pode_ter_saido_papel()
    {
        var job = AutomaticJob();
        job.Lease(AgentId, Now);

        Assert.True(job.MayHaveBeenPrinted);
        job.Cancel(Now);

        Assert.Equal(PrintJobStatus.Cancelled, job.Status);
        Assert.Throws<InvalidOperationException>(() => job.Lease(AgentId, Now));
    }

    private static PrintJob AutomaticJob() =>
        new(Guid.CreateVersion7(), Guid.CreateVersion7(), PrintJobReason.Automatic, Document(), Now);

    private static PrintDocument Document() => PrintDocument.Of([PrintLine.Title("PEDIDO #1"), PrintLine.Cut]);
}
