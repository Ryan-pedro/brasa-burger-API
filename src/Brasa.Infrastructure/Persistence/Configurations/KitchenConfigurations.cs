using Brasa.Domain.Catalog;
using Brasa.Domain.Kitchen;
using Brasa.Domain.Ordering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Brasa.Infrastructure.Persistence.Configurations;

internal sealed class KitchenTicketConfiguration : IEntityTypeConfiguration<KitchenTicket>
{
    public void Configure(EntityTypeBuilder<KitchenTicket> builder)
    {
        builder.ToTable("tickets", "kitchen");

        builder.HasKey(ticket => ticket.Id);
        builder.Property(ticket => ticket.Id).ValueGeneratedNever();
        builder.Property<uint>("Version").IsRowVersion();

        builder.Property(ticket => ticket.OrderType).HasConversion<string>().HasMaxLength(20);
        builder.Property(ticket => ticket.Document).HasConversion<string>().HasMaxLength(20);
        builder.Property(ticket => ticket.Preparation).HasConversion<string>().HasMaxLength(20);
        builder.Property(ticket => ticket.CustomerLabel).HasMaxLength(OrderCustomer.NameMaxLength).IsRequired();
        builder.Property(ticket => ticket.Notes).HasMaxLength(Order.NotesMaxLength);

        builder.HasOne<Order>().WithMany().HasForeignKey(ticket => ticket.OrderId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(ticket => ticket.OrderId).IsUnique();
        builder.HasIndex(ticket => new { ticket.Document, ticket.IssuedAt });

        builder.OwnsMany(ticket => ticket.Items, item =>
        {
            item.ToTable("ticket_items", "kitchen");
            item.WithOwner().HasForeignKey("TicketId");
            item.Property<int>("Id");
            item.HasKey("Id");
            item.Property(i => i.Name).HasMaxLength(Product.NameMaxLength).IsRequired();
            item.Property(i => i.Notes).HasMaxLength(CartLine.NotesMaxLength);
        });
    }
}
