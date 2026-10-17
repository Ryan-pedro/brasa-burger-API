namespace Brasa.Application.Ordering.Tracking;

public sealed record TrackingItem(string Name, int Quantity, long TotalInCents, IReadOnlyList<string> Options, string? Notes);
