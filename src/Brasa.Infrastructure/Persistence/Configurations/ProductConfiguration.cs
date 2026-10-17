using Brasa.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Brasa.Infrastructure.Persistence.Configurations;

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
