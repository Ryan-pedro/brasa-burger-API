using Brasa.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Brasa.Infrastructure.Persistence.Configurations;

internal sealed class OptionGroupConfiguration : IEntityTypeConfiguration<OptionGroup>
{
    public void Configure(EntityTypeBuilder<OptionGroup> builder)
    {
        builder.ToTable("option_groups", "catalog");

        builder.HasKey(group => group.Id);
        builder.Property(group => group.Id).ValueGeneratedNever();
        builder.Property(group => group.Name).HasMaxLength(OptionGroup.NameMaxLength).IsRequired();
        builder.Property(group => group.Kind).HasConversion<string>().HasMaxLength(20);

        builder.OwnsMany(group => group.Options, option =>
        {
            option.ToTable("options", "catalog");
            option.WithOwner().HasForeignKey("OptionGroupId");
            option.HasKey(o => o.Id);
            option.Property(o => o.Id).ValueGeneratedNever();
            option.Property(o => o.Name).HasMaxLength(Option.NameMaxLength).IsRequired();
            option.Property(o => o.PriceDelta).HasPrecision(10, 2);
        });
    }
}
