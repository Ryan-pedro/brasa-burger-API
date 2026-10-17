using Brasa.Domain.Catalog;

namespace Brasa.Domain.Ordering;

public sealed record PricedOption(Guid OptionId, string GroupName, string OptionName, OptionKind Kind, decimal PriceDelta);
