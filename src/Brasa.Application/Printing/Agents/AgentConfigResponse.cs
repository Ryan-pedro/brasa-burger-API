namespace Brasa.Application.Printing.Agents;

public sealed record AgentConfigResponse(DateTimeOffset ServerTime, int ProtocolVersion, IReadOnlyList<AgentPrinterConfig> Printers);
