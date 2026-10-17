using Brasa.Application.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Payments.Online;

public sealed class ReconcilePixPaymentHandler(IAppDbContext db, PixPaymentSettlement settlement)
{
    public async Task HandleAsync(Guid paymentId, CancellationToken cancellationToken)
    {
        var payment = await db.Payments.FirstAsync(p => p.Id == paymentId, cancellationToken);
        await settlement.SettleAsync(payment, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }
}
