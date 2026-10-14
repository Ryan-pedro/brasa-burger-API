using Brasa.Application.Ordering.Shared;
using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Ordering;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Ordering.Management;

public sealed class ChangeOrderStatusHandler(IAppDbContext db, AuditTrail audit, TimeProvider clock)
{
    public async Task<Result<OrderDetailResponse>> HandleAsync(Guid id, OrderAction action, ReasonRequest? request, CancellationToken cancellationToken)
    {
        var needsReason = action is OrderAction.Reject or OrderAction.Cancel;
        var reason = request?.Reason?.Trim();

        if (needsReason && (string.IsNullOrEmpty(reason) || reason.Length > Order.ReasonMaxLength))
        {
            var errors = new FieldErrors();
            errors.Add("reason", $"Informe o motivo, com até {Order.ReasonMaxLength} caracteres.");
            return errors.ToError();
        }

        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (order is null)
        {
            return OrderErrors.NotFound;
        }

        if (!OrderActions.Available(order, payment: null).Contains(OrderActions.Name(action)))
        {
            return OrderErrors.InvalidTransition;
        }

        var now = clock.GetUtcNow();
        switch (action)
        {
            case OrderAction.Accept:
                order.Confirm(now);
                break;
            case OrderAction.Reject:
                order.Reject(reason!, now);
                break;
            case OrderAction.Cancel:
                order.Cancel(reason!, now);
                break;
            case OrderAction.Complete:
                order.Complete(now);
                break;
        }

        audit.Record($"order.{OrderActions.Name(action)}", order.Id, needsReason ? new { reason } : null);
        await db.SaveChangesAsync(cancellationToken);

        return await db.DetailAsync(order, cancellationToken);
    }
}
