using Brasa.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Brasa.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories", "catalog");

        builder.HasKey(category => category.Id);
        builder.Property(category => category.Id).ValueGeneratedNever();

        builder.Property(category => category.Name).HasMaxLength(Category.NameMaxLength).IsRequired();
        builder.Property(category => category.Slug).HasMaxLength(Category.NameMaxLength).IsRequired();
        builder.HasIndex(category => category.Slug).IsUnique();

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(category => category.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products", "catalog");

        builder.HasKey(product => product.Id);
        builder.Property(product => product.Id).ValueGeneratedNever();

        builder.Property(product => product.Name).HasMaxLength(Product.NameMaxLength).IsRequired();
        builder.Property(product => product.Description).HasMaxLength(Product.DescriptionMaxLength).IsRequired();
        builder.Property(product => product.Price).HasPrecision(10, 2);
        builder.Property(product => product.Size).HasMaxLength(Product.SizeMaxLength);
        builder.Property(product => product.ImageUrl).HasMaxLength(Product.ImageUrlMaxLength);

        // Uma categoria com produtos não pode ser apagada: primeiro mova ou remova os produtos.
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(product => new { product.CategoryId, product.SortOrder });

        builder.Ignore(product => product.OptionGroupIds);
        builder.OwnsMany(product => product.OptionGroups, link =>
        {
            link.ToTable("product_option_groups", "catalog");
            link.WithOwner().HasForeignKey("ProductId");
            link.HasKey("ProductId", nameof(ProductOptionGroup.OptionGroupId));
            link.HasOne<OptionGroup>().WithMany().HasForeignKey(l => l.OptionGroupId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}

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
