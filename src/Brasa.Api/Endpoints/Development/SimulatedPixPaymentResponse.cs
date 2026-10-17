namespace Brasa.Api.Endpoints.Development;

public sealed record SimulatedPixPaymentResponse(string Body, IReadOnlyDictionary<string, string> Headers, bool Delivered);
