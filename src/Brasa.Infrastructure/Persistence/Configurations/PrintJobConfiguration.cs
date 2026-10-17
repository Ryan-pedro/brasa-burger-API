using System.Text.Json;
using Brasa.Domain.Kitchen;
using Brasa.Domain.Printing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Brasa.Infrastructure.Persistence.Configurations;

internal sealed class PrintJobConfiguration : IEntityTypeConfiguration<PrintJob>
{
    private static readonly JsonSerializerOptions DocumentJson = JsonSerializerOptions.Web;

    public void Configure(EntityTypeBuilder<PrintJob> builder)
    {
        builder.ToTable("jobs", "printing");

        builder.HasKey(job => job.Id);
        builder.Property(job => job.Id).ValueGeneratedNever();
        builder.Property<uint>("Version").IsRowVersion();

        builder.Property(job => job.Reason).HasConversion<string>().HasMaxLength(30);
        builder.Property(job => job.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(job => job.LastError).HasMaxLength(PrintAttempt.ErrorMaxLength);
        builder.Property(job => job.Document)
            .HasColumnType("jsonb")
            .HasConversion(
                document => JsonSerializer.Serialize(document, DocumentJson),
                json => JsonSerializer.Deserialize<PrintDocument>(json, DocumentJson)!,
                new ValueComparer<PrintDocument>((left, right) => ReferenceEquals(left, right), document => document.GetHashCode(), document => document));

        builder.Ignore(job => job.CurrentAttempt);
        builder.Ignore(job => job.MayHaveBeenPrinted);

        builder.HasOne<Printer>().WithMany().HasForeignKey(job => job.PrinterId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<KitchenTicket>().WithMany().HasForeignKey(job => job.TicketId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(job => new { job.Status, job.NextAttemptAt });
        builder.HasIndex(job => new { job.TicketId, job.PrinterId, job.Reason })
            .IsUnique()
            .HasFilter("reason IN ('Automatic', 'CancellationNotice')");

        builder.OwnsMany(job => job.Attempts, attempt =>
        {
            attempt.ToTable("attempts", "printing");
            attempt.WithOwner().HasForeignKey("PrintJobId");
            attempt.HasKey(a => a.Id);
            attempt.Property(a => a.Id).ValueGeneratedNever();
            attempt.Property(a => a.Outcome).HasConversion<string>().HasMaxLength(20);
            attempt.Property(a => a.Error).HasMaxLength(PrintAttempt.ErrorMaxLength);
            attempt.Ignore(a => a.IsOpen);
        });
    }
}
