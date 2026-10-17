namespace Brasa.Application.Catalog.Menu;

public sealed record MenuOption(Guid Id, string Name, long PriceDeltaInCents, bool IsAvailable);
