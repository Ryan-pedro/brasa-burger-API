namespace Brasa.Application.Catalog.OptionGroups;

public sealed record OptionRequest(Guid? Id, string? Name, long? PriceDeltaInCents);
