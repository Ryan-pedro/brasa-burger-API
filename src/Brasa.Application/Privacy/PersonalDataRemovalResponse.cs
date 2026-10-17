namespace Brasa.Application.Privacy;

public sealed record PersonalDataRemovalResponse(int RemovedOrders, IReadOnlyList<int> UnfinishedOrderNumbers);
