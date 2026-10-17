using Brasa.Application.Printing.Jobs;
using Brasa.Application.Printing.Layouts;
using Brasa.Application.Shared.Persistence;
using Brasa.Application.Shared.Results;
using Brasa.Application.StoreSettings.Shared;
using Brasa.Domain.Printing;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Printing.Printers;

public sealed class PrintTestPageHandler(IAppDbContext db, TimeProvider clock)
{
    public async Task<Result<PrintJobResponse>> HandleAsync(Guid printerId, CancellationToken cancellationToken)
    {
        var printer = await db.Printers.AsNoTracking().FirstOrDefaultAsync(p => p.Id == printerId, cancellationToken);
        if (printer is null)
        {
            return PrintingErrors.PrinterNotFound;
        }

        var store = await db.CurrentStoreAsync(cancellationToken);
        if (store is null)
        {
            return StoreErrors.NotConfigured;
        }

        var now = clock.GetUtcNow();
        var job = new PrintJob(printer.Id, null, PrintJobReason.Test, TestPageLayout.Build(printer, store.Name, store.ToStoreTime(now)), now);

        db.PrintJobs.Add(job);
        await db.SaveChangesAsync(cancellationToken);

        return PrintJobResponse.From(job);
    }
}
