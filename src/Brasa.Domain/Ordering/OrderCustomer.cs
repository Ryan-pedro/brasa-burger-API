using Brasa.Domain.Shared;

namespace Brasa.Domain.Ordering;

public sealed record OrderCustomer
{
    public const int NameMaxLength = 80;
    public const int PhoneMaxLength = 20;
    public const int TableMaxLength = 10;

    private OrderCustomer()
    {
    }

    public OrderCustomer(string? name, string? phone, string? table)
    {
        Name = FreeText.Clean(name, NameMaxLength, nameof(name));
        Phone = FreeText.Clean(phone, PhoneMaxLength, nameof(phone));
        Table = FreeText.Clean(table, TableMaxLength, nameof(table));
    }

    public string? Name { get; private init; }

    public string? Phone { get; private init; }

    public string? Table { get; private init; }
}
