using Brasa.Application.Shared.Observability;
using Brasa.Application.Shared.Persistence;
using Brasa.Domain.Payments;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Payments.Online;

public sealed class ReceivePixWebhookHandler(
    IAppDbContext db,
    IPixGateway gateway,
    IDatabaseErrors databaseErrors,
    PaymentWebhookSignal signal,
    BrasaMetrics metrics,
    TimeProvider clock)
{
    public async Task<PixWebhookReceipt> HandleAsync(PixWebhookMessage message, CancellationToken cancellationToken)
    {
        var notification = gateway.IsConfigured ? gateway.ReadWebhook(message) : null;
        if (notification is null)
        {
            metrics.PixWebhookReceived("rejected");
            return PixWebhookReceipt.Rejected;
        }

        db.PaymentWebhookEvents.Add(new PaymentWebhookEvent(gateway.Provider, notification.EventId, notification.ChargeId, clock.GetUtcNow()));

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (databaseErrors.IsUniqueViolation(exception))
        {
            metrics.PixWebhookReceived("duplicate");
            return PixWebhookReceipt.Duplicate;
        }

        metrics.PixWebhookReceived("accepted");
        signal.Notify();
        return PixWebhookReceipt.Accepted;
    }
}
