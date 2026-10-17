using System.Net;
using System.Net.Http.Json;
using Brasa.Application.Kitchen.Board;
using Brasa.Application.Kitchen.Tickets;
using Brasa.Application.Ordering.Management;
using Brasa.Application.Ordering.Tracking;
using Brasa.Application.Printing.Agents;
using Brasa.Application.Printing.Jobs;
using Brasa.Application.Printing.Layouts;
using Brasa.Application.Shared.Outbox;
using Brasa.Application.Shared.Realtime;
using Brasa.Domain.Printing;

namespace Brasa.IntegrationTests;

[Collection(ApiCollection.Name)]
public class PrintingTests(StoreApiFactory factory)
{
    [Fact]
    public async Task Pedido_confirmado_vira_tarefa_que_o_agente_reserva_e_confirma()
    {
        var agent = await TestPrintAgent.InstallAsync(factory);
        var order = await PlaceOrderAsync();

        var job = Assert.Single(await agent.LeaseAsync(), job => job.Reason == "Automatic");
        var printed = await (await agent.ReportAsync(job, "Printed")).ReadAsync<PrintJobResponse>();
        var repeated = await agent.ReportAsync(job, "Printed");

        Assert.Equal(agent.Printer.Id, job.PrinterId);
        Assert.Contains(job.Document.Lines, line => line.Text == $"PEDIDO #{order.Number}");
        Assert.Equal("Printed", printed.Status);
        Assert.Equal(HttpStatusCode.OK, repeated.StatusCode);
        Assert.Equal("Printed", (await BoardTicketAsync(order.Number)).PrintStatus);
    }

    [Fact]
    public async Task Falha_volta_para_a_fila_so_depois_da_espera()
    {
        var agent = await TestPrintAgent.InstallAsync(factory);
        await PlaceOrderAsync();
        var job = Assert.Single(await agent.LeaseAsync());

        var failed = await (await agent.ReportAsync(job, "Failed", "Sem papel")).ReadAsync<PrintJobResponse>();
        var tooSoon = await agent.LeaseAsync();
        factory.Clock.Advance(TimeSpan.FromSeconds(6));
        var retried = Assert.Single(await agent.LeaseAsync());

        Assert.Equal("Pending", failed.Status);
        Assert.Equal("Sem papel", failed.LastError);
        Assert.Empty(tooSoon);
        Assert.Equal(job.JobId, retried.JobId);
        Assert.NotEqual(job.AttemptId, retried.AttemptId);
    }

    [Fact]
    public async Task Reserva_vencida_volta_para_a_fila_e_resultado_desconhecido_marca_segunda_via()
    {
        var agent = await TestPrintAgent.InstallAsync(factory);
        await PlaceOrderAsync();
        var first = Assert.Single(await agent.LeaseAsync());

        factory.Clock.Advance(PrintJobPolicy.LeaseDuration + TimeSpan.FromSeconds(10));
        await factory.RunJobAsync<RecoverPrintJobsJob>();
        factory.Clock.Advance(TimeSpan.FromSeconds(6));
        var second = Assert.Single(await agent.LeaseAsync());
        await agent.ReportAsync(second, "Unknown");
        factory.Clock.Advance(TimeSpan.FromSeconds(20));
        var third = Assert.Single(await agent.LeaseAsync());

        Assert.Equal(first.JobId, second.JobId);
        Assert.False(second.PossibleDuplicate);
        Assert.True(third.PossibleDuplicate);
    }

    [Fact]
    public async Task Reimpressao_pela_cozinha_sai_marcada_como_segunda_via_e_fica_na_auditoria()
    {
        var agent = await TestPrintAgent.InstallAsync(factory);
        var order = await PlaceOrderAsync();
        var kitchen = await factory.SignedInAsync(StoreApiFactory.Kitchen);
        var ticket = await BoardTicketAsync(order.Number);

        var reprint = await (await kitchen.PostAsync($"/api/v1/kitchen/tickets/{ticket.Id}/reprint", null)).ReadAsync<PrintJobResponse>();
        var managerOnKitchen = await (await factory.SignedInAsync(StoreApiFactory.Manager)).PostAsync($"/api/v1/kitchen/tickets/{ticket.Id}/reprint", null);

        var copy = Assert.Single(await agent.LeaseAsync(), job => job.JobId == reprint.Id);
        Assert.Equal("Reprint", copy.Reason);
        Assert.Equal(KitchenTicketLayout.CopyMark, copy.Document.Lines[0].Text);
        Assert.Equal(HttpStatusCode.Forbidden, managerOnKitchen.StatusCode);
    }

    [Fact]
    public async Task Cancelar_pedido_ja_impresso_gera_aviso_de_cancelamento()
    {
        var agent = await TestPrintAgent.InstallAsync(factory);
        var order = await PlaceOrderAsync();
        await agent.ReportAsync(Assert.Single(await agent.LeaseAsync()), "Printed");
        var manager = await factory.SignedInAsync(StoreApiFactory.Manager);

        var orderId = (await manager.GetFromJsonAsync<OrdersPage>("/api/v1/admin/orders?limit=100"))!.Items.Single(o => o.Number == order.Number).Id;
        (await manager.PostAsJsonAsync($"/api/v1/admin/orders/{orderId}/cancel", new { reason = "Cliente desistiu" })).EnsureSuccessStatusCode();

        var notice = Assert.Single(await agent.LeaseAsync());
        Assert.Equal("CancellationNotice", notice.Reason);
        Assert.Contains(notice.Document.Lines, line => line.Text == "*** CANCELADO ***");
    }

    [Fact]
    public async Task Codigo_de_pareamento_vale_uma_vez_e_segredo_errado_nao_ganha_token()
    {
        var owner = await factory.SignedInAsync(StoreApiFactory.Owner);
        var client = TestPrintAgent.NewAgentClient(factory);
        var code = await TestPrintAgent.PairingCodeAsync(owner);

        var first = await client.PostAsJsonAsync("/api/v1/agent/pair", new { code, name = "Caixa", agentVersion = "1.0.0" });
        var reused = await client.PostAsJsonAsync("/api/v1/agent/pair", new { code, name = "Caixa 2", agentVersion = "1.0.0" });
        var paired = await first.ReadAsync<PairAgentResponse>();
        var wrongSecret = await client.PostAsJsonAsync("/api/v1/agent/token", new { agentId = paired.AgentId, secret = "errado" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, reused.StatusCode);
        Assert.Equal("pairing_code_invalid", await reused.ReadErrorCodeAsync());
        Assert.Equal(HttpStatusCode.Unauthorized, wrongSecret.StatusCode);
        Assert.Equal("agent_credentials_invalid", await wrongSecret.ReadErrorCodeAsync());
    }

    [Fact]
    public async Task Agente_desatualizado_ou_sem_token_e_recusado()
    {
        var withoutProtocol = await factory.CreateClient().PostAsJsonAsync("/api/v1/agent/pair", new { code = "AAAA-BBBB", name = "Caixa", agentVersion = "1.0.0" });
        var withoutToken = await TestPrintAgent.NewAgentClient(factory).PostAsJsonAsync("/api/v1/agent/jobs/lease", new { maxJobs = 1 });

        Assert.Equal(HttpStatusCode.UpgradeRequired, withoutProtocol.StatusCode);
        Assert.Equal("agent_protocol_outdated", await withoutProtocol.ReadErrorCodeAsync());
        Assert.Equal(HttpStatusCode.Unauthorized, withoutToken.StatusCode);
    }

    [Fact]
    public async Task Agente_revogado_perde_o_acesso_e_as_tarefas_abertas_sao_canceladas()
    {
        var agent = await TestPrintAgent.InstallAsync(factory);
        await PlaceOrderAsync();
        var owner = await factory.SignedInAsync(StoreApiFactory.Owner);

        var revoked = await (await owner.PostAsync($"/api/v1/admin/printing/agents/{agent.AgentId}/revoke", null)).ReadAsync<PrintAgentResponse>();
        var lease = await agent.Client.PostAsJsonAsync("/api/v1/agent/jobs/lease", new { maxJobs = 1 });
        var newToken = await agent.Client.PostAsJsonAsync("/api/v1/agent/token", new { agentId = agent.AgentId, secret = agent.Secret });
        var cancelled = await owner.GetFromJsonAsync<PrintJobsPage>("/api/v1/admin/printing/jobs?status=Cancelled&limit=100");

        Assert.True(revoked.IsRevoked);
        Assert.All(revoked.Printers, printer => Assert.False(printer.IsActive));
        Assert.Equal(HttpStatusCode.Unauthorized, lease.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, newToken.StatusCode);
        Assert.Contains(cancelled!.Items, job => job.PrinterId == agent.Printer.Id);
    }

    [Fact]
    public async Task Sinal_de_vida_atualiza_a_impressora_e_o_silencio_deixa_o_agente_offline()
    {
        var agent = await TestPrintAgent.InstallAsync(factory);
        var owner = await factory.SignedInAsync(StoreApiFactory.Owner);

        var config = await (await agent.Client.PostAsJsonAsync("/api/v1/agent/heartbeat", new
            {
                agentVersion = "1.0.1",
                localQueueSize = 0,
                printers = new[] { new { printerId = agent.Printer.Id, status = "PaperOut", message = "Sem papel" } },
            }))
            .ReadAsync<AgentConfigResponse>();

        var board = await (await factory.SignedInAsync(StoreApiFactory.Kitchen)).GetFromJsonAsync<BoardResponse>("/api/v1/kitchen/tickets");
        factory.Clock.Advance(PrintAgent.OfflineAfter + TimeSpan.FromSeconds(1));
        await factory.RunJobAsync<MarkSilentAgentsOfflineJob>();
        var agents = await owner.GetFromJsonAsync<List<PrintAgentResponse>>("/api/v1/admin/printing/agents");

        var printerConfig = Assert.Single(config.Printers);
        Assert.Equal(TestPrintAgent.PaperAddress, printerConfig.Address);
        Assert.Equal(48, printerConfig.Columns);
        Assert.Contains(board!.Printers, printer => printer.Id == agent.Printer.Id && printer.Status == "PaperOut");
        Assert.False(agents!.Single(a => a.Id == agent.AgentId).IsOnline);
    }

    [Fact]
    public async Task Nova_tarefa_avisa_somente_o_agente_dono_da_impressora()
    {
        var agent = await TestPrintAgent.InstallAsync(factory);
        var owner = await factory.SignedInAsync(StoreApiFactory.Owner);

        (await owner.PostAsync($"/api/v1/admin/printing/printers/{agent.Printer.Id}/test", null)).EnsureSuccessStatusCode();
        await factory.RunJobAsync<DispatchOutboxJob>();

        Assert.Contains(factory.Notifier.Sent, sent =>
            sent.Channel == RealtimeChannels.Agent && sent.Notice.Type == RealtimeNoticeTypes.PrintJobsAvailable && sent.Notice.Id == agent.AgentId);
        Assert.Equal("Test", Assert.Single(await agent.LeaseAsync()).Reason);
    }

    [Fact]
    public async Task Sem_impressora_padrao_a_reimpressao_responde_409_e_a_cozinha_nao_gerencia_impressao()
    {
        var owner = await factory.SignedInAsync(StoreApiFactory.Owner);
        var agents = await owner.GetFromJsonAsync<List<PrintAgentResponse>>("/api/v1/admin/printing/agents");
        foreach (var active in agents!.Where(a => !a.IsRevoked))
        {
            (await owner.PostAsync($"/api/v1/admin/printing/agents/{active.Id}/revoke", null)).EnsureSuccessStatusCode();
        }

        var order = await PlaceOrderAsync();
        var ticket = await BoardTicketAsync(order.Number);
        var reprint = await owner.PostAsync($"/api/v1/admin/printing/tickets/{ticket.Id}/reprint", null);
        var kitchenOnAdmin = await (await factory.SignedInAsync(StoreApiFactory.Kitchen)).GetAsync("/api/v1/admin/printing/agents");

        Assert.Null(ticket.PrintStatus);
        Assert.Equal(HttpStatusCode.Conflict, reprint.StatusCode);
        Assert.Equal("no_default_printer", await reprint.ReadErrorCodeAsync());
        Assert.Equal(HttpStatusCode.Forbidden, kitchenOnAdmin.StatusCode);
    }

    private async Task<OrderTrackingResponse> PlaceOrderAsync()
    {
        var product = await (await factory.SignedInAsync(StoreApiFactory.Manager)).ProductAsync(1500);
        return await (await factory.CreateClient().PlaceOrderAsync(1500, [TestData.Item(product.Id)])).ReadAsync<OrderTrackingResponse>();
    }

    private async Task<TicketResponse> BoardTicketAsync(int orderNumber)
    {
        var board = await (await factory.SignedInAsync(StoreApiFactory.Kitchen)).GetFromJsonAsync<BoardResponse>("/api/v1/kitchen/tickets");
        return board!.Tickets.Single(ticket => ticket.OrderNumber == orderNumber);
    }
}
