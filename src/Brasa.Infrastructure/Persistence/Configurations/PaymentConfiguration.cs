using Brasa.Domain.Ordering;
using Brasa.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Brasa.Infrastructure.Persistence.Configurations;

internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments", "payments");

        builder.HasKey(payment => payment.Id);
        builder.Property(payment => payment.Id).ValueGeneratedNever();
        builder.Property<uint>("Version").IsRowVersion();

        builder.Property(payment => payment.Method).HasConversion<string>().HasMaxLength(20);
        builder.Property(payment => payment.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(payment => payment.Amount).HasPrecision(10, 2);
        builder.Property(payment => payment.Provider).HasMaxLength(Payment.ProviderMaxLength);
        builder.Property(payment => payment.ChargeId).HasMaxLength(Payment.ChargeIdMaxLength);
        builder.Property(payment => payment.PixCopyPaste).HasMaxLength(Payment.PixCopyPasteMaxLength);

        builder.HasOne<Order>().WithMany().HasForeignKey(payment => payment.OrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(payment => payment.OrderId).IsUnique();
        builder.HasIndex(payment => new { payment.Provider, payment.ChargeId }).IsUnique().HasFilter("charge_id IS NOT NULL");
        builder.HasIndex(payment => new { payment.Method, payment.Status });
    }
}
