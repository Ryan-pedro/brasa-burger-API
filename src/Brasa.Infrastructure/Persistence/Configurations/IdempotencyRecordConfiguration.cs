using Brasa.Application.Shared.Idempotency;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Brasa.Infrastructure.Persistence.Configurations;

internal sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("idempotency_records", "ordering");

        builder.HasKey(record => record.Key);
        builder.Property(record => record.Key).HasMaxLength(IdempotencyRecord.KeyMaxLength);
        builder.Property(record => record.RequestHash).HasMaxLength(64).IsRequired();
        builder.HasIndex(record => record.CreatedAt);
    }
}
