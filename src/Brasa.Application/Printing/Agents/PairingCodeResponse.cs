namespace Brasa.Application.Printing.Agents;

public sealed record PairingCodeResponse(string Code, DateTimeOffset ExpiresAt);
