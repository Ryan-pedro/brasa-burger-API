using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Idempotency;
using Brasa.Application.Shared.Outbox;
using Brasa.Domain.Catalog;
using Brasa.Domain.Kitchen;
using Brasa.Domain.Ordering;
using Brasa.Domain.Payments;
using Brasa.Domain.StoreSettings;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Application.Shared.Persistence;

/// <summary>
/// Acesso a dados usado pelos casos de uso. A implementação fica em Infrastructure.
/// </summary>
public interface IAppDbContext
{
    DbSet<Store> Stores { get; }

    DbSet<Category> Categories { get; }

    DbSet<Product> Products { get; }

    DbSet<OptionGroup> OptionGroups { get; }

    DbSet<Offer> Offers { get; }

    DbSet<Order> Orders { get; }

    DbSet<Payment> Payments { get; }

    DbSet<KitchenTicket> KitchenTickets { get; }

    DbSet<OutboxMessage> OutboxMessages { get; }

    DbSet<AuditEntry> AuditEntries { get; }

    DbSet<IdempotencyRecord> IdempotencyRecords { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
