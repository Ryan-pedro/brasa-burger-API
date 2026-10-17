namespace Brasa.Domain.Ordering;

public sealed class OrderItem
{
    private readonly List<OrderItemOption> _options = [];

    private OrderItem()
    {
    }

    internal OrderItem(CartLine line)
    {
        Id = Guid.CreateVersion7();
        ProductId = line.ProductId;
        ProductName = line.ProductName;
        BasePrice = line.BasePrice;
        UnitPrice = line.UnitPrice;
        Quantity = line.Quantity;
        Total = line.Total;
        Notes = line.Notes;
        _options.AddRange(line.Options.Select(option => new OrderItemOption(option)));
    }

    public Guid Id { get; private set; }

    public Guid ProductId { get; private set; }

    public string ProductName { get; private set; } = null!;

    public decimal BasePrice { get; private set; }

    public decimal UnitPrice { get; private set; }

    public int Quantity { get; private set; }

    public decimal Total { get; private set; }

    public string? Notes { get; private set; }

    public IReadOnlyList<OrderItemOption> Options => _options;

    internal void ClearNotes() => Notes = null;
}
