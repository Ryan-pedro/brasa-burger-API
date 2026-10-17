namespace Brasa.Domain.Kitchen;

public sealed class TicketItem
{
    private TicketItem()
    {
    }

    public TicketItem(int quantity, string name, IReadOnlyList<string> modifiers, string? notes)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 1);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(modifiers);

        Quantity = quantity;
        Name = name;
        Modifiers = [.. modifiers];
        Notes = notes;
    }

    public int Quantity { get; private set; }

    public string Name { get; private set; } = null!;

    public List<string> Modifiers { get; private set; } = [];

    public string? Notes { get; private set; }

    internal void ClearNotes() => Notes = null;
}
