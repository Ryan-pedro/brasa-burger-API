using Brasa.Domain.Ordering;
using Brasa.Domain.Shared;

namespace Brasa.Domain.Kitchen;

public sealed class KitchenTicket : AggregateRoot
{
    private readonly List<TicketItem> _items = [];

    private KitchenTicket()
    {
    }

    public KitchenTicket(Guid orderId, int orderNumber, OrderType orderType, string customerLabel, string? notes, IReadOnlyList<TicketItem> items, DateTimeOffset now)
    {
        if (orderId == Guid.Empty)
        {
            throw new ArgumentException("A comanda precisa de um pedido.", nameof(orderId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(customerLabel);
        ArgumentNullException.ThrowIfNull(items);

        if (items.Count == 0)
        {
            throw new ArgumentException("A comanda precisa de itens.", nameof(items));
        }

        Id = Guid.CreateVersion7();
        OrderId = orderId;
        OrderNumber = orderNumber;
        OrderType = orderType;
        CustomerLabel = customerLabel;
        Notes = notes;
        Document = TicketDocumentStatus.Issued;
        Preparation = PreparationStatus.Queued;
        IssuedAt = now;
        UpdatedAt = now;
        _items.AddRange(items);

        Raise(new TicketChanged(this));
    }

    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public int OrderNumber { get; private set; }

    public OrderType OrderType { get; private set; }

    public string CustomerLabel { get; private set; } = null!;

    public string? Notes { get; private set; }

    public TicketDocumentStatus Document { get; private set; }

    public PreparationStatus Preparation { get; private set; }

    public DateTimeOffset IssuedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyList<TicketItem> Items => _items;

    public bool CanAdvance => Document == TicketDocumentStatus.Issued && Preparation != PreparationStatus.Dispatched;

    public bool CanUndo => Document == TicketDocumentStatus.Issued
        && Preparation is PreparationStatus.InPreparation or PreparationStatus.Ready;

    public void Advance(DateTimeOffset now)
    {
        if (!CanAdvance)
        {
            throw new InvalidOperationException("A comanda não pode avançar.");
        }

        ChangePreparation(Preparation + 1, now);
    }

    public void Undo(DateTimeOffset now)
    {
        if (!CanUndo)
        {
            throw new InvalidOperationException("Não há passo para desfazer.");
        }

        ChangePreparation(Preparation - 1, now);
    }

    public void Void(DateTimeOffset now) => ChangeDocument(TicketDocumentStatus.Voided, now);

    public void Close(DateTimeOffset now) => ChangeDocument(TicketDocumentStatus.Closed, now);

    private void ChangePreparation(PreparationStatus preparation, DateTimeOffset now)
    {
        Preparation = preparation;
        UpdatedAt = now;

        Raise(new PreparationChanged(this));
        Raise(new TicketChanged(this));
    }

    private void ChangeDocument(TicketDocumentStatus document, DateTimeOffset now)
    {
        if (Document != TicketDocumentStatus.Issued)
        {
            return;
        }

        Document = document;
        UpdatedAt = now;

        Raise(new TicketChanged(this));
    }
}
