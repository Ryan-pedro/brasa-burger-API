using Brasa.Domain.Shared;

namespace Brasa.Domain.Printing;

public sealed class Printer : AggregateRoot
{
    public const int NameMaxLength = 60;
    public const int AddressMaxLength = 200;
    public const int StatusMessageMaxLength = 200;

    private Printer()
    {
    }

    public Printer(Guid agentId, string name, PaperWidth paperWidth, string address)
    {
        if (agentId == Guid.Empty)
        {
            throw new ArgumentException("A impressora precisa de um agente.", nameof(agentId));
        }

        Id = Guid.CreateVersion7();
        AgentId = agentId;
        Change(name, paperWidth, address);
        IsActive = true;
        Status = PrinterStatus.Unknown;
    }

    public Guid Id { get; private set; }

    public Guid AgentId { get; private set; }

    public string Name { get; private set; } = null!;

    public PaperWidth PaperWidth { get; private set; }

    public string Address { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public bool IsDefault { get; private set; }

    public PrinterStatus Status { get; private set; }

    public string? StatusMessage { get; private set; }

    public DateTimeOffset? StatusReportedAt { get; private set; }

    public int Columns => PaperWidth == PaperWidth.Mm58 ? 32 : 48;

    public void Change(string name, PaperWidth paperWidth, string address)
    {
        Name = FreeText.Required(name, NameMaxLength, nameof(name));
        Address = FreeText.Required(address, AddressMaxLength, nameof(address));
        PaperWidth = paperWidth;
    }

    public void Activate() => IsActive = true;

    public void Deactivate()
    {
        IsActive = false;
        IsDefault = false;
    }

    public void MakeDefault()
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Uma impressora inativa não pode ser a padrão.");
        }

        IsDefault = true;
    }

    public void ClearDefault() => IsDefault = false;

    public void ReportStatus(PrinterStatus status, string? message, DateTimeOffset now)
    {
        var cleanMessage = FreeText.Clean(message, StatusMessageMaxLength, nameof(message));
        var changed = status != Status || cleanMessage != StatusMessage;

        Status = status;
        StatusMessage = cleanMessage;
        StatusReportedAt = now;

        if (changed)
        {
            Raise(new PrinterStatusChanged(this));
        }
    }
}
