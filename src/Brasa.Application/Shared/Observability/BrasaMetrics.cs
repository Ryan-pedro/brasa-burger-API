using System.Diagnostics.Metrics;

namespace Brasa.Application.Shared.Observability;

public sealed class BrasaMetrics
{
    public const string MeterName = "Brasa";

    private readonly Counter<long> _ordersCreated;
    private readonly Counter<long> _loginFailures;
    private readonly Counter<long> _pixWebhooks;
    private readonly Counter<long> _paymentAlerts;
    private OperationalSnapshot _snapshot = OperationalSnapshot.Empty;

    public BrasaMetrics(IMeterFactory meters)
    {
        var meter = meters.Create(MeterName);
        _ordersCreated = meter.CreateCounter<long>("brasa.orders.created", description: "Pedidos criados no checkout.");
        _loginFailures = meter.CreateCounter<long>("brasa.auth.login_failures", description: "Tentativas de login recusadas.");
        _pixWebhooks = meter.CreateCounter<long>("brasa.payments.pix_webhooks", description: "Avisos de Pix recebidos, por resultado.");
        _paymentAlerts = meter.CreateCounter<long>("brasa.payments.alerts", description: "Pagamentos que exigem atenção da loja.");
        meter.CreateObservableGauge("brasa.outbox.oldest_pending_age", () => _snapshot.OutboxOldestPendingSeconds, "s", "Idade do aviso mais antigo ainda não entregue.");
        meter.CreateObservableGauge("brasa.printing.pending_jobs", () => _snapshot.PendingPrintJobs, description: "Tarefas de impressão esperando um agente.");
        meter.CreateObservableGauge("brasa.printing.oldest_pending_age", () => _snapshot.PrintOldestPendingSeconds, "s", "Idade da tarefa de impressão pendente mais antiga.");
        meter.CreateObservableGauge("brasa.printing.online_agents", () => _snapshot.OnlinePrintAgents, description: "Agentes de impressão conectados.");
        meter.CreateObservableGauge("brasa.payments.waiting_pix", () => _snapshot.WaitingPixPayments, description: "Pix online aguardando pagamento.");
    }

    public void OrderCreated(string type, string paymentMethod) =>
        _ordersCreated.Add(1, new KeyValuePair<string, object?>("type", type), new KeyValuePair<string, object?>("payment_method", paymentMethod));

    public void LoginFailed(string reason) => _loginFailures.Add(1, new KeyValuePair<string, object?>("reason", reason));

    public void PixWebhookReceived(string outcome) => _pixWebhooks.Add(1, new KeyValuePair<string, object?>("outcome", outcome));

    public void PaymentAlert(string kind) => _paymentAlerts.Add(1, new KeyValuePair<string, object?>("kind", kind));

    public void Record(OperationalSnapshot snapshot) => _snapshot = snapshot;
}
