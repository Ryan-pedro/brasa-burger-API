using Brasa.Domain.Printing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Brasa.Infrastructure.Persistence.Configurations;

internal sealed class PairingCodeConfiguration : IEntityTypeConfiguration<PairingCode>
{
    public void Configure(EntityTypeBuilder<PairingCode> builder)
    {
        builder.ToTable("pairing_codes", "printing");

        builder.HasKey(code => code.Id);
        builder.Property(code => code.Id).ValueGeneratedNever();
        builder.Property<uint>("Version").IsRowVersion();

        builder.Property(code => code.CodeHash).HasMaxLength(64).IsRequired();
        builder.HasIndex(code => code.CodeHash).IsUnique();
    }
}
