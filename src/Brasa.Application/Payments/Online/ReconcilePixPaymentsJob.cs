using Brasa.Application.Shared.Jobs;
using Brasa.Application.Shared.Persistence;
using Brasa.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Brasa.Application.Payments.Online;

public sealed class ReconcilePixPaymentsJob(IAppDbContext db, IServiceScopeFactory scopes, ILogger<ReconcilePixPaymentsJob> logger, TimeProvider clock) : IJob
{
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var settledBefore = now - PixPaymentPolicy.ReconcileAfter;

        var pending = await db.Payments
            .AsNoTracking()
            .Where(p => p.Method == PaymentMethod.PixOnline
                && p.Status == PaymentStatus.Pending
                && (p.CreatedAt <= settledBefore || p.ExpiresAt <= now))
            .OrderBy(p => p.CreatedAt)
            .Select(p => p.Id)
            .ToListAsync(cancellationToken);

        foreach (var paymentId in pending)
        {
            try
            {
                await using var scope = scopes.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<ReconcilePixPaymentHandler>().HandleAsync(paymentId, cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Falha ao conferir o pagamento {PaymentId} no gateway.", paymentId);
            }
        }
    }
}
