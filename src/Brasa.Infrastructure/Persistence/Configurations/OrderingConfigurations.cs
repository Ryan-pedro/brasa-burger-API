using Brasa.Application.Shared.Idempotency;
using Brasa.Domain.Catalog;
using Brasa.Domain.Ordering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Brasa.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public const string NumberSequence = "order_numbers";

    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders", "ordering");

        builder.HasKey(order => order.Id);
        builder.Property(order => order.Id).ValueGeneratedNever();
        builder.Property<uint>("Version").IsRowVersion();

        builder.HasIndex(order => order.Number).IsUnique();
        builder.Property(order => order.TrackingToken).HasMaxLength(Order.TrackingTokenLength).IsRequired();
        builder.HasIndex(order => order.TrackingToken).IsUnique();
        builder.HasIndex(order => new { order.Status, order.CreatedAt });

        builder.Property(order => order.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(order => order.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(order => order.PaymentMethod).HasConversion<string>().HasMaxLength(20);
        builder.Property(order => order.Notes).HasMaxLength(Order.NotesMaxLength);
        builder.Property(order => order.StatusReason).HasMaxLength(Order.ReasonMaxLength);
        builder.Property(order => order.DiscountLabel).HasMaxLength(Offer.LabelMaxLength);
        builder.Property(order => order.Currency).HasMaxLength(3).IsRequired();
        builder.Property(order => order.ChangeFor).HasPrecision(10, 2);
        builder.Property(order => order.Subtotal).HasPrecision(10, 2);
        builder.Property(order => order.DiscountAmount).HasPrecision(10, 2);
        builder.Property(order => order.DeliveryFee).HasPrecision(10, 2);
        builder.Property(order => order.Total).HasPrecision(10, 2);

        builder.OwnsOne(order => order.Customer, customer =>
        {
            customer.Property(c => c.Name).HasColumnName("customer_name").HasMaxLength(OrderCustomer.NameMaxLength);
            customer.Property(c => c.Phone).HasColumnName("customer_phone").HasMaxLength(OrderCustomer.PhoneMaxLength);
            customer.Property(c => c.Table).HasColumnName("customer_table").HasMaxLength(OrderCustomer.TableMaxLength);
        });
        builder.Navigation(order => order.Customer).IsRequired();

        builder.OwnsOne(order => order.DeliveryAddress, address =>
        {
            address.Property(a => a.Street).HasColumnName("address_street").HasMaxLength(DeliveryAddress.StreetMaxLength);
            address.Property(a => a.Number).HasColumnName("address_number").HasMaxLength(DeliveryAddress.NumberMaxLength);
            address.Property(a => a.Complement).HasColumnName("address_complement").HasMaxLength(DeliveryAddress.ComplementMaxLength);
            address.Property(a => a.District).HasColumnName("address_district").HasMaxLength(DeliveryAddress.DistrictMaxLength);
        });

        builder.OwnsMany(order => order.Items, item =>
        {
            item.ToTable("order_items", "ordering");
            item.WithOwner().HasForeignKey("OrderId");
            item.HasKey(i => i.Id);
            item.Property(i => i.Id).ValueGeneratedNever();
            item.Property(i => i.ProductName).HasMaxLength(Product.NameMaxLength).IsRequired();
            item.Property(i => i.Notes).HasMaxLength(CartLine.NotesMaxLength);
            item.Property(i => i.BasePrice).HasPrecision(10, 2);
            item.Property(i => i.UnitPrice).HasPrecision(10, 2);
            item.Property(i => i.Total).HasPrecision(10, 2);

            item.OwnsMany(i => i.Options, option =>
            {
                option.ToTable("order_item_options", "ordering");
                option.WithOwner().HasForeignKey("OrderItemId");
                option.Property<int>("Id");
                option.HasKey("Id");
                option.Property(o => o.GroupName).HasMaxLength(OptionGroup.NameMaxLength).IsRequired();
                option.Property(o => o.OptionName).HasMaxLength(Option.NameMaxLength).IsRequired();
                option.Property(o => o.Kind).HasConversion<string>().HasMaxLength(20);
                option.Property(o => o.PriceDelta).HasPrecision(10, 2);
            });
        });
    }
}

internal sealed class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable("idempotency_records", "ordering");

        builder.HasKey(record => record.Key);
        builder.Property(record => record.Key).HasMaxLength(IdempotencyRecord.KeyMaxLength);
        builder.Property(record => record.RequestHash).HasMaxLength(64).IsRequired();
        builder.HasIndex(record => record.CreatedAt);
    }
}
