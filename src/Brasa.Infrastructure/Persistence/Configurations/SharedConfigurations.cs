using Brasa.Application.Shared.Audit;
using Brasa.Application.Shared.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Brasa.Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages", "shared");

        builder.HasKey(message => message.Id);
        builder.Property(message => message.Id).ValueGeneratedNever();
        builder.Property(message => message.Channel).HasMaxLength(30).IsRequired();
        builder.Property(message => message.Type).HasMaxLength(60).IsRequired();
        builder.Property(message => message.LastError).HasMaxLength(OutboxMessage.ErrorMaxLength);
        builder.HasIndex(message => message.CreatedAt).HasFilter("processed_at IS NULL");
    }
}

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
