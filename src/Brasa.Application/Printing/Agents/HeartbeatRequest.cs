namespace Brasa.Application.Printing.Agents;

public sealed record HeartbeatRequest(string? AgentVersion, int? LocalQueueSize, string? LastError, IReadOnlyList<PrinterStatusReport>? Printers);
