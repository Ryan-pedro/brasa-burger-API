using Brasa.Application.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Payments.Online;

public sealed class ProcessPaymentWebhookHandler(IAppDbContext db, PixPaymentSettlement settlement, TimeProvider clock)
{
    public async Task HandleAsync(Guid webhookEventId, CancellationToken cancellationToken)
    {
        var webhookEvent = await db.PaymentWebhookEvents.FirstAsync(e => e.Id == webhookEventId, cancellationToken);
        if (!webhookEvent.IsWaiting)
        {
            return;
        }

        var payment = await db.Payments.FirstOrDefaultAsync(p => p.Provider == webhookEvent.Provider && p.ChargeId == webhookEvent.ChargeId, cancellationToken);
        if (payment is not null)
        {
            await settlement.SettleAsync(payment, cancellationToken);
        }

        webhookEvent.MarkProcessed(clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
    }
}
