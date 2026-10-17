using System.Security.Cryptography;
using Brasa.Domain.Payments;
using Brasa.Domain.Shared;

namespace Brasa.Domain.Ordering;

public sealed class Order : AggregateRoot
{
    public const int NotesMaxLength = 280;
    public const int ReasonMaxLength = 200;
    public const int MaxItems = 50;
    public const int TrackingTokenLength = 32;
    public const string PaymentExpiredReason = "O Pix não foi pago no prazo.";
    public const string PaymentMismatchReason = "O Pix foi pago com valor diferente do pedido. A loja vai entrar em contato.";
    public const string DefaultCurrency = "BRL";

    private readonly List<OrderItem> _items = [];

    private Order()
    {
    }

    public Order(
        int number,
        OrderType type,
        OrderCustomer customer,
        DeliveryAddress? address,
        string? notes,
        PaymentMethod paymentMethod,
        decimal? changeFor,
        IReadOnlyList<CartLine> lines,
        PriceBreakdown price,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(lines);
        ArgumentNullException.ThrowIfNull(price);

        if (lines.Count is 0 or > MaxItems)
        {
            throw new ArgumentException($"O pedido precisa de 1 a {MaxItems} itens.", nameof(lines));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(number, 1);
        RequireContact(type, customer, address);

        if (changeFor is not null && (paymentMethod != PaymentMethod.Cash || changeFor < price.Total))
        {
            throw new ArgumentOutOfRangeException(nameof(changeFor), changeFor, "Troco só para pagamento em dinheiro, com valor que cubra o total.");
        }

        Id = Guid.CreateVersion7();
        Number = number;
        TrackingToken = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(TrackingTokenLength / 2));
        Type = type;
        Status = paymentMethod == PaymentMethod.PixOnline ? OrderStatus.AwaitingPayment : OrderStatus.Placed;
        Customer = customer;
        DeliveryAddress = address;
        Notes = FreeText.Clean(notes, NotesMaxLength, nameof(notes));
        PaymentMethod = paymentMethod;
        ChangeFor = changeFor;
        Subtotal = price.Subtotal;
        DiscountLabel = price.Discount?.Label;
        DiscountAmount = price.DiscountAmount;
        DeliveryFee = price.DeliveryFee;
        Total = price.Total;
        Currency = DefaultCurrency;
        CreatedAt = now;
        UpdatedAt = now;
        _items.AddRange(lines.Select(line => new OrderItem(line)));

        Raise(new OrderPlaced(this));
    }

    public Guid Id { get; private set; }

    public int Number { get; private set; }

    public string TrackingToken { get; private set; } = null!;

    public OrderType Type { get; private set; }

    public OrderStatus Status { get; private set; }

    public OrderCustomer Customer { get; private set; } = null!;

    public DeliveryAddress? DeliveryAddress { get; private set; }

    public string? Notes { get; private set; }

    public PaymentMethod PaymentMethod { get; private set; }

    public decimal? ChangeFor { get; private set; }

    public decimal Subtotal { get; private set; }

    public string? DiscountLabel { get; private set; }

    public decimal DiscountAmount { get; private set; }

    public decimal DeliveryFee { get; private set; }

    public decimal Total { get; private set; }

    public string Currency { get; private set; } = null!;

    public string? StatusReason { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public DateTimeOffset? PersonalDataRemovedAt { get; private set; }

    public IReadOnlyList<OrderItem> Items => _items;

    public bool IsFinished => OrderTransitions.IsFinal(Status);

    public bool CanMoveTo(OrderStatus target) => OrderTransitions.IsAllowed(Status, target);

    public void ConfirmPayment(DateTimeOffset now) => MoveTo(OrderStatus.Placed, now, null);

    public void ExpirePayment(DateTimeOffset now) => MoveTo(OrderStatus.PaymentExpired, now, PaymentExpiredReason);

    public void RefusePaymentWithWrongAmount(DateTimeOffset now) => MoveTo(OrderStatus.PaymentExpired, now, PaymentMismatchReason);

    public void Confirm(DateTimeOffset now) => MoveTo(OrderStatus.Confirmed, now, null);

    public void Reject(string reason, DateTimeOffset now) => MoveTo(OrderStatus.Rejected, now, RequireReason(reason));

    public void Cancel(string reason, DateTimeOffset now) => MoveTo(OrderStatus.Cancelled, now, RequireReason(reason));

    public void Complete(DateTimeOffset now) => MoveTo(OrderStatus.Completed, now, null);

    public void Dispatch(DateTimeOffset now) =>
        MoveTo(Type == OrderType.Delivery ? OrderStatus.OutForDelivery : OrderStatus.Completed, now, null);

    public void FollowPreparation(OrderStatus status, DateTimeOffset now)
    {
        if (status is not (OrderStatus.Confirmed or OrderStatus.InPreparation or OrderStatus.Ready))
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "O preparo só leva o pedido a confirmado, em preparação ou pronto.");
        }

        MoveTo(status, now, null);
    }

    public void RemovePersonalData(DateTimeOffset now)
    {
        if (!IsFinished)
        {
            throw new InvalidOperationException("Os dados pessoais só são removidos de pedidos encerrados.");
        }

        if (PersonalDataRemovedAt is not null)
        {
            return;
        }

        Customer = Customer.WithoutPersonalData();
        DeliveryAddress = null;
        Notes = null;
        foreach (var item in _items)
        {
            item.ClearNotes();
        }

        PersonalDataRemovedAt = now;
    }

    private void MoveTo(OrderStatus target, DateTimeOffset now, string? reason)
    {
        if (!CanMoveTo(target))
        {
            throw new InvalidOperationException($"O pedido não pode passar de {Status} para {target}.");
        }

        var previous = Status;
        Status = target;
        StatusReason = reason;
        UpdatedAt = now;

        Raise(new OrderStatusChanged(this, previous));
    }

    private static string RequireReason(string reason) => FreeText.Required(reason, ReasonMaxLength, nameof(reason));

    private static void RequireContact(OrderType type, OrderCustomer customer, DeliveryAddress? address)
    {
        var complete = type switch
        {
            OrderType.DineIn => customer.Table is not null && address is null,
            OrderType.Pickup => customer.Name is not null && address is null,
            OrderType.Delivery => customer.Name is not null && customer.Phone is not null && address is not null,
            _ => false,
        };

        if (!complete)
        {
            throw new ArgumentException("Os dados do cliente não correspondem ao tipo de pedido.", nameof(customer));
        }
    }
}
