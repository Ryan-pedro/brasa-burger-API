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
