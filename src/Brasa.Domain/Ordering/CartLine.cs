namespace Brasa.Domain.Ordering;

public sealed record CartLine(
    Guid ProductId,
    string ProductName,
    decimal BasePrice,
    int Quantity,
    IReadOnlyList<PricedOption> Options,
    string? Notes)
{
    public const int MaxQuantity = 99;

    public const int NotesMaxLength = 140;

    public decimal UnitPrice => BasePrice + Options.Sum(option => option.PriceDelta);

    public decimal Total => UnitPrice * Quantity;
}
