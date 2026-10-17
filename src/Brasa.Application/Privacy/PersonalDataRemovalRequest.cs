namespace Brasa.Application.Privacy;

public sealed record PersonalDataRemovalRequest(string? Phone, IReadOnlyList<int>? OrderNumbers);
