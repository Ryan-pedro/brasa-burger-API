using Brasa.Domain.Printing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Brasa.Infrastructure.Persistence.Configurations;

internal sealed class PrinterConfiguration : IEntityTypeConfiguration<Printer>
{
    public void Configure(EntityTypeBuilder<Printer> builder)
    {
        builder.ToTable("printers", "printing");

        builder.HasKey(printer => printer.Id);
        builder.Property(printer => printer.Id).ValueGeneratedNever();

        builder.Property(printer => printer.Name).HasMaxLength(Printer.NameMaxLength).IsRequired();
        builder.Property(printer => printer.Address).HasMaxLength(Printer.AddressMaxLength).IsRequired();
        builder.Property(printer => printer.PaperWidth).HasConversion<string>().HasMaxLength(10);
        builder.Property(printer => printer.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(printer => printer.StatusMessage).HasMaxLength(Printer.StatusMessageMaxLength);
        builder.Ignore(printer => printer.Columns);

        builder.HasOne<PrintAgent>().WithMany().HasForeignKey(printer => printer.AgentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(printer => printer.IsDefault).IsUnique().HasFilter("is_default");
    }
}
