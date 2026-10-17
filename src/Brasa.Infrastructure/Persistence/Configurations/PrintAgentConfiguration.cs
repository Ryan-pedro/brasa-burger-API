using Brasa.Domain.Printing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Brasa.Infrastructure.Persistence.Configurations;

internal sealed class PrintAgentConfiguration : IEntityTypeConfiguration<PrintAgent>
{
    public void Configure(EntityTypeBuilder<PrintAgent> builder)
    {
        builder.ToTable("agents", "printing");

        builder.HasKey(agent => agent.Id);
        builder.Property(agent => agent.Id).ValueGeneratedNever();
        builder.Property<uint>("Version").IsRowVersion();

        builder.Property(agent => agent.Name).HasMaxLength(PrintAgent.NameMaxLength).IsRequired();
        builder.Property(agent => agent.AgentVersion).HasMaxLength(PrintAgent.VersionMaxLength).IsRequired();
        builder.Property(agent => agent.SecretHash).HasMaxLength(64).IsRequired();
        builder.Property(agent => agent.LastError).HasMaxLength(PrintAgent.LastErrorMaxLength);
        builder.Ignore(agent => agent.IsRevoked);
    }
}
