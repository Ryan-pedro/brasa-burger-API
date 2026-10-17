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
