using Brasa.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Brasa.Infrastructure.Persistence.Configurations;

internal sealed class OfferConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> builder)
    {
        builder.ToTable("offers", "catalog");

        builder.HasKey(offer => offer.Id);
        builder.Property(offer => offer.Id).ValueGeneratedNever();
        builder.Property(offer => offer.Label).HasMaxLength(Offer.LabelMaxLength).IsRequired();
        builder.Property(offer => offer.Kind).HasConversion<string>().HasMaxLength(20);
        builder.Property(offer => offer.PromoPrice).HasPrecision(10, 2);
        builder.Property(offer => offer.MinimumSubtotal).HasPrecision(10, 2);
        builder.Property(offer => offer.DiscountAmount).HasPrecision(10, 2);

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(offer => offer.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(offer => new { offer.IsActive, offer.StartsAt });
    }
}
