using Brasa.Domain.Printing;

namespace Brasa.Application.Printing.Agents;

public sealed record PrintAgentResponse(
    Guid Id,
    string Name,
    string AgentVersion,
    bool IsOnline,
    bool IsRevoked,
    DateTimeOffset PairedAt,
    DateTimeOffset LastSeenAt,
    int LocalQueueSize,
    string? LastError,
    IReadOnlyList<PrinterResponse> Printers)
{
    public static PrintAgentResponse From(PrintAgent agent, IEnumerable<Printer> printers) =>
        new(
            agent.Id,
            agent.Name,
            agent.AgentVersion,
            agent.IsOnline,
            agent.IsRevoked,
            agent.PairedAt,
            agent.LastSeenAt,
            agent.LocalQueueSize,
            agent.LastError,
            printers.OrderBy(printer => printer.Name).Select(PrinterResponse.From).ToList());
}
