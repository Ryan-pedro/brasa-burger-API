using Brasa.Application.Shared;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Domain.Printing;
using Brasa.Domain.Shared;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Printing.Printers;

internal static class PrinterRules
{
    public static ValidPrinter? Validate(PrinterRequest request, FieldErrors errors)
    {
        var name = FreeText.Normalize(request.Name);
        if (name is null || name.Length > Printer.NameMaxLength)
        {
            errors.Add("name", $"Informe o nome da impressora, com até {Printer.NameMaxLength} caracteres.");
        }

        if (!EnumText.TryParse<PaperWidth>(request.PaperWidth, out var paperWidth))
        {
            errors.Add("paperWidth", $"Larguras aceitas: {EnumText.Names<PaperWidth>()}.");
        }

        var address = FreeText.Normalize(request.Address);
        if (address is null || address.Length > Printer.AddressMaxLength)
        {
            errors.Add("address", $"Informe o endereço da impressora (ex.: tcp://192.168.0.50:9100), com até {Printer.AddressMaxLength} caracteres.");
        }

        if (request.IsDefault == true && request.IsActive == false)
        {
            errors.Add("isDefault", "Uma impressora inativa não pode ser a padrão.");
        }

        return errors.Any ? null : new ValidPrinter(name!, paperWidth, address!);
    }

    public static async Task ApplyFlagsAsync(this IAppDbContext db, Printer printer, PrinterRequest request, CancellationToken cancellationToken)
    {
        if (request.IsActive == false)
        {
            printer.Deactivate();
        }
        else if (request.IsActive == true)
        {
            printer.Activate();
        }

        if (request.IsDefault == true)
        {
            var currentDefaults = await db.Printers.Where(p => p.IsDefault && p.Id != printer.Id).ToListAsync(cancellationToken);
            foreach (var other in currentDefaults)
            {
                other.ClearDefault();
            }

            printer.MakeDefault();
        }
        else if (request.IsDefault == false)
        {
            printer.ClearDefault();
        }
    }
}
