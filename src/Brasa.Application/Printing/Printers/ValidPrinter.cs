using Brasa.Domain.Printing;

namespace Brasa.Application.Printing.Printers;

internal sealed record ValidPrinter(string Name, PaperWidth PaperWidth, string Address);
