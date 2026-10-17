using Brasa.Application.Shared.Jobs;
using Brasa.Application.Shared.Persistence;
using Brasa.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Brasa.Application.Payments.Online;

public sealed class ProcessPaymentWebhooksJob(IAppDbContext db, IServiceScopeFactory scopes, ILogger<ProcessPaymentWebhooksJob> logger) : IJob
{
    public const int BatchSize = 50;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var waiting = await db.PaymentWebhookEvents
            .AsNoTracking()
            .Where(e => e.ProcessedAt == null && e.Attempts < PaymentWebhookEvent.MaxAttempts)
            .OrderBy(e => e.ReceivedAt)
            .Select(e => e.Id)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var webhookEventId in waiting)
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<ProcessPaymentWebhookHandler>().HandleAsync(webhookEventId, cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Falha ao processar o aviso de pagamento {WebhookEventId}.", webhookEventId);
                await using var scope = scopes.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<RecordPaymentWebhookFailureHandler>().HandleAsync(webhookEventId, exception, cancellationToken);
            }
        }
    }
}
