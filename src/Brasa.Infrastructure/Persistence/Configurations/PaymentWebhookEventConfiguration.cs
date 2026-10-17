using Brasa.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Brasa.Infrastructure.Persistence.Configurations;

internal sealed class PaymentWebhookEventConfiguration : IEntityTypeConfiguration<PaymentWebhookEvent>
{
    public void Configure(EntityTypeBuilder<PaymentWebhookEvent> builder)
    {
        builder.ToTable("webhook_events", "payments");

        builder.HasKey(webhookEvent => webhookEvent.Id);
        builder.Property(webhookEvent => webhookEvent.Id).ValueGeneratedNever();

        builder.Property(webhookEvent => webhookEvent.Provider).HasMaxLength(Payment.ProviderMaxLength).IsRequired();
        builder.Property(webhookEvent => webhookEvent.EventId).HasMaxLength(PaymentWebhookEvent.EventIdMaxLength).IsRequired();
        builder.Property(webhookEvent => webhookEvent.ChargeId).HasMaxLength(Payment.ChargeIdMaxLength).IsRequired();
        builder.Property(webhookEvent => webhookEvent.LastError).HasMaxLength(PaymentWebhookEvent.ErrorMaxLength);

        builder.HasIndex(webhookEvent => new { webhookEvent.Provider, webhookEvent.EventId }).IsUnique();
        builder.HasIndex(webhookEvent => webhookEvent.ReceivedAt).HasFilter("processed_at IS NULL");
    }
}
