using Brasa.Domain.StoreSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Brasa.Infrastructure.Persistence;

public static class ProductionDatabase
{
    public static async Task MigrateAsync(IServiceProvider services, StoreSetupOptions store, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync(cancellationToken);

        if (await db.Stores.AnyAsync(cancellationToken))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(store.Name))
        {
            throw new InvalidOperationException("Configure Store:Name para criar a loja na primeira implantação.");
        }

        db.Stores.Add(new Store(store.Name, store.TimeZoneId));
        await db.SaveChangesAsync(cancellationToken);
    }
}
