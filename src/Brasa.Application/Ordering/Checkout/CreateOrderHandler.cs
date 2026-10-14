using System.Security.Cryptography;
using System.Text.Json;
using Brasa.Application.Ordering.Shared;
using Brasa.Application.Ordering.Tracking;
using Brasa.Application.Shared;
using Brasa.Application.Shared.Idempotency;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Ordering;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Ordering.Checkout;

public sealed record CreatedOrder(OrderTrackingResponse Order, bool Replayed);

public sealed class CreateOrderHandler(
    IAppDbContext db,
    CartBuilder cart,
    IOrderNumbers numbers,
    IDatabaseErrors databaseErrors,
    TimeProvider clock)
{
    public async Task<Result<CreatedOrder>> HandleAsync(string? idempotencyKey, CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var key = idempotencyKey?.Trim();
        if (string.IsNullOrEmpty(key) || key.Length > IdempotencyRecord.KeyMaxLength)
        {
            var errors = new FieldErrors();
            errors.Add("idempotencyKey", $"Envie o cabeçalho Idempotency-Key com até {IdempotencyRecord.KeyMaxLength} caracteres.");
            return errors.ToError();
        }

        var requestHash = Convert.ToHexStringLower(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(request, JsonSerializerOptions.Web)));

        var replay = await ReplayAsync(key, requestHash, cancellationToken);
        if (replay is not null)
        {
            return replay;
        }

        var created = await PlaceAsync(request, cancellationToken);
        if (!created.IsSuccess)
        {
            return created.Error!;
        }

        var order = created.Value!;
        db.Orders.Add(order);
        db.IdempotencyRecords.Add(new IdempotencyRecord(key, requestHash, order.Id, clock.GetUtcNow()));

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (databaseErrors.IsUniqueViolation(exception))
        {
            var concurrent = await ReplayAsync(key, requestHash, cancellationToken);
            if (concurrent is null)
            {
                throw;
            }

            return concurrent;
        }

        return new CreatedOrder(OrderTrackingResponse.From(order), Replayed: false);
    }

    private async Task<Result<Order>> PlaceAsync(CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var priced = await cart.BuildAsync(request.Type, request.Items, cancellationToken);
        if (!priced.IsSuccess)
        {
            return priced.Error!;
        }

        var (store, type, lines, price) = priced.Value!;

        var errors = new FieldErrors();
        var details = CreateOrderRules.Validate(request, type, errors);
        if (details is null)
        {
            return errors.ToError();
        }

        if (details.ChangeFor < price.Total)
        {
            errors.Add("changeForInCents", "O troco precisa ser para um valor maior ou igual ao total.");
            return errors.ToError();
        }

        if (Money.ToCents(price.Total) != request.ExpectedTotalInCents)
        {
            return new Error(
                ErrorKind.Conflict,
                "cart_changed",
                "Os valores do carrinho mudaram. Confira a nova cotação.",
                Extensions: new Dictionary<string, object?> { ["quote"] = QuoteResponse.From(priced.Value!) });
        }

        if (price.Subtotal < store.MinimumOrder)
        {
            return OrderErrors.BelowMinimum(Money.ToCents(store.MinimumOrder));
        }

        var now = clock.GetUtcNow();
        var order = new Order(
            await numbers.NextAsync(cancellationToken),
            type,
            details.Customer,
            details.Address,
            details.Notes,
            details.PaymentMethod,
            details.ChangeFor,
            lines,
            price,
            now);

        if (!store.ManualAcceptance)
        {
            order.Confirm(now);
        }

        return order;
    }

    private async Task<Result<CreatedOrder>?> ReplayAsync(string key, string requestHash, CancellationToken cancellationToken)
    {
        var record = await db.IdempotencyRecords.AsNoTracking().FirstOrDefaultAsync(r => r.Key == key, cancellationToken);
        if (record is null)
        {
            return null;
        }

        if (record.RequestHash != requestHash)
        {
            return OrderErrors.IdempotencyKeyReused;
        }

        var order = await db.Orders.AsNoTracking().FirstAsync(o => o.Id == record.ResultId, cancellationToken);
        return new CreatedOrder(OrderTrackingResponse.From(order), Replayed: true);
    }
}
