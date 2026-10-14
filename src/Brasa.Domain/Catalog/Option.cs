namespace Brasa.Domain.Catalog;

public sealed class Option
{
    public const int NameMaxLength = 60;
    public const decimal MaxPriceDelta = 999.99m;

    private Option()
    {
    }

    internal Option(string name, decimal priceDelta, int sortOrder)
    {
        Id = Guid.CreateVersion7();
        Change(name, priceDelta, sortOrder);
        IsAvailable = true;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = null!;

    public decimal PriceDelta { get; private set; }

    public bool IsAvailable { get; private set; }

    public int SortOrder { get; private set; }

    internal void Change(string name, decimal priceDelta, int sortOrder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var trimmed = name.Trim();
        if (trimmed.Length > NameMaxLength)
        {
            throw new ArgumentException($"O nome da opção pode ter no máximo {NameMaxLength} caracteres.", nameof(name));
        }

        if (priceDelta < 0 || priceDelta > MaxPriceDelta || decimal.Round(priceDelta, 2) != priceDelta)
        {
            throw new ArgumentOutOfRangeException(nameof(priceDelta), priceDelta, $"O acréscimo vai de zero a {MaxPriceDelta}, com no máximo duas casas.");
        }

        Name = trimmed;
        PriceDelta = priceDelta;
        SortOrder = sortOrder;
    }

    internal void SetAvailability(bool isAvailable) => IsAvailable = isAvailable;
}
