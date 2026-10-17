using Brasa.Domain.Printing;

namespace Brasa.Application.Printing.Layouts;

public static class TestPageLayout
{
    public static PrintDocument Build(Printer printer, string storeName, DateTimeOffset nowAtStoreTime) =>
        PrintDocument.Of(
        [
            PrintLine.Title("TESTE DE IMPRESSÃO"),
            PrintLine.Centered(storeName),
            PrintLine.Separator,
            PrintLine.Plain($"Impressora: {printer.Name}"),
            PrintLine.Plain($"Papel: {(printer.PaperWidth == PaperWidth.Mm58 ? "58 mm" : "80 mm")}, {printer.Columns} colunas"),
            PrintLine.Plain(nowAtStoreTime.ToString("dd/MM/yyyy HH:mm:ss")),
            PrintLine.Separator,
            PrintLine.Plain("Acentuação: ação, pão, você, açúcar, coração"),
            PrintLine.Strong("Negrito"),
            PrintLine.Title("Grande"),
            PrintLine.Feed,
            PrintLine.Cut,
        ]);
}
