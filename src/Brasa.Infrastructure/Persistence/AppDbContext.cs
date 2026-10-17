using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Idempotency;
using Brasa.Application.Shared.Outbox;
using Brasa.Application.Shared.Persistence;
using Brasa.Domain.Catalog;
using Brasa.Domain.Identity;
using Brasa.Domain.Kitchen;
using Brasa.Domain.Ordering;
using Brasa.Domain.Payments;
using Brasa.Domain.Printing;
using Brasa.Domain.StoreSettings;
using Brasa.Infrastructure.Identity;
using Brasa.Infrastructure.Persistence.Configurations;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Brasa.Infrastructure.Persistence;

/// <remarks>
/// Herda de <see cref="IdentityUserContext{TUser, TKey}"/> para ganhar as tabelas de usuários
/// do ASP.NET Core Identity. Não usa as tabelas de papéis do Identity: o papel de cada
/// pessoa fica no próprio usuário (<see cref="AppUser.Role"/>).
/// </remarks>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options, DomainEventDispatcher dispatcher, OutboxSignal outboxSignal)
    : IdentityUserContext<AppUser, Guid>(options), IAppDbContext, IDataProtectionKeyContext
{
    public DbSet<Store> Stores => Set<Store>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<OptionGroup> OptionGroups => Set<OptionGroup>();

    public DbSet<Offer> Offers => Set<Offer>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<PaymentWebhookEvent> PaymentWebhookEvents => Set<PaymentWebhookEvent>();

    public DbSet<KitchenTicket> KitchenTickets => Set<KitchenTicket>();

    public DbSet<PrintAgent> PrintAgents => Set<PrintAgent>();

    public DbSet<PairingCode> PairingCodes => Set<PairingCode>();

    public DbSet<Printer> Printers => Set<Printer>();

    public DbSet<PrintJob> PrintJobs => Set<PrintJob>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    public DbSet<StaffInvitation> StaffInvitations => Set<StaffInvitation>();

    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        await dispatcher.DispatchAsync(this, cancellationToken);

        var queuesNotice = ChangeTracker.Entries<OutboxMessage>().Any(entry => entry.State == EntityState.Added);
        var saved = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

        if (queuesNotice)
        {
            outboxSignal.Notify();
        }

        return saved;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tabelas do Identity com nomes próprios, no schema "identity".
        modelBuilder.Entity<AppUser>(user =>
        {
            user.ToTable("users", "identity");
            user.Property(u => u.DisplayName).HasMaxLength(120).IsRequired();
            user.Property(u => u.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
            user.Property(u => u.IsActive).IsRequired();
        });
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims", "identity");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins", "identity");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens", "identity");

        modelBuilder.HasSequence<int>(OrderConfiguration.NumberSequence, "ordering");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
