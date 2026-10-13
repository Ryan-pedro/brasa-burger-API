using Brasa.Domain.StoreSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Brasa.Infrastructure.Persistence.Configurations;

internal sealed class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("stores", "store");

        builder.HasKey(store => store.Id);
        builder.Property(store => store.Id).ValueGeneratedNever();

        builder.Property(store => store.Name).HasMaxLength(Store.NameMaxLength).IsRequired();
        builder.Property(store => store.TimeZoneId).HasMaxLength(64).IsRequired();
    }
}
