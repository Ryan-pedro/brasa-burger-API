using Brasa.Application.Shared.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Brasa.Infrastructure.Persistence.Configurations;

internal sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("entries", "audit");

        builder.HasKey(entry => entry.Id);
        builder.Property(entry => entry.Id).ValueGeneratedNever();
        builder.Property(entry => entry.Action).HasMaxLength(AuditEntry.ActionMaxLength).IsRequired();
        builder.Property(entry => entry.Data).HasColumnType("jsonb");
        builder.HasIndex(entry => entry.OccurredAt);
        builder.HasIndex(entry => entry.SubjectId);
    }
}
