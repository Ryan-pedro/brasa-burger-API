using Brasa.Domain.Printing;

namespace Brasa.Application.Printing.Agents;

public sealed record AgentPrinterConfig(Guid Id, string Name, string PaperWidth, int Columns, string Address, bool IsActive, bool IsDefault)
{
    public static AgentPrinterConfig From(Printer printer) =>
        new(printer.Id, printer.Name, printer.PaperWidth.ToString(), printer.Columns, printer.Address, printer.IsActive, printer.IsDefault);
}
