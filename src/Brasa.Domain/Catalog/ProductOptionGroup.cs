namespace Brasa.Domain.Catalog;

public sealed class ProductOptionGroup
{
    private ProductOptionGroup()
    {
    }

    internal ProductOptionGroup(Guid optionGroupId, int sortOrder)
    {
        OptionGroupId = optionGroupId;
        SortOrder = sortOrder;
    }

    public Guid OptionGroupId { get; private set; }

    public int SortOrder { get; internal set; }
}
