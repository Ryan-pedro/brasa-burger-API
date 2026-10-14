using Brasa.Application.Ordering.Shared;
using Brasa.Application.Shared;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Ordering;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Ordering.Management;

public sealed class ListOrdersHandler(IAppDbContext db)
{
    public const int DefaultLimit = 50;
    public const int MaxLimit = 100;

    public async Task<Result<OrdersPage>> HandleAsync(string[]? status, int? limit, string? cursor, CancellationToken cancellationToken)
    {
        var errors = new FieldErrors();

        var statuses = new List<OrderStatus>();
        foreach (var text in status ?? [])
        {
            if (EnumText.TryParse<OrderStatus>(text, out var parsed))
            {
                statuses.Add(parsed);
            }
            else
            {
                errors.Add("status", $"Situações aceitas: {EnumText.Names<OrderStatus>()}.");
            }
        }

        if (limit is < 1 or > MaxLimit)
        {
            errors.Add("limit", $"O limite vai de 1 a {MaxLimit}.");
        }

        int? before = null;
        if (cursor is not null)
        {
            if (int.TryParse(cursor, out var number) && number > 0)
            {
                before = number;
            }
            else
            {
                errors.Add("cursor", "Cursor inválido.");
            }
        }

        if (errors.Any)
        {
            return errors.ToError();
        }

        var pageSize = limit ?? DefaultLimit;
        var query = db.Orders.AsNoTracking();

        if (statuses.Count > 0)
        {
            query = query.Where(order => statuses.Contains(order.Status));
        }

        if (before is { } beforeNumber)
        {
            query = query.Where(order => order.Number < beforeNumber);
        }

        var orders = await query
            .OrderByDescending(order => order.Number)
            .Take(pageSize + 1)
            .ToListAsync(cancellationToken);

        var page = orders.Take(pageSize).ToList();
        var orderIds = page.Select(order => order.Id).ToList();

        var payments = await db.Payments
            .AsNoTracking()
            .Where(payment => orderIds.Contains(payment.OrderId))
            .ToDictionaryAsync(payment => payment.OrderId, cancellationToken);

        var items = page
            .Select(order => new OrderSummary(
                order.Id,
                order.Number,
                order.Status.ToString(),
                order.Type.ToString(),
                OrderLabels.Customer(order),
                order.Items.Sum(item => item.Quantity),
                Money.ToCents(order.Total),
                order.PaymentMethod.ToString(),
                payments.GetValueOrDefault(order.Id)?.Status.ToString(),
                order.CreatedAt))
            .ToList();

        var nextCursor = orders.Count > pageSize ? page[^1].Number.ToString() : null;
        return new OrdersPage(items, nextCursor);
    }
}
