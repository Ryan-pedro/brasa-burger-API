using Brasa.Application.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Payments.Online;

public sealed class RecordPaymentWebhookFailureHandler(IAppDbContext db)
{
    public async Task HandleAsync(Guid webhookEventId, Exception failure, CancellationToken cancellationToken)
    {
        var webhookEvent = await db.PaymentWebhookEvents.FirstAsync(e => e.Id == webhookEventId, cancellationToken);
        webhookEvent.RecordFailure($"{failure.GetType().Name}: {failure.Message}");
        await db.SaveChangesAsync(cancellationToken);
    }
}
