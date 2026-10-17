using Brasa.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Brasa.Infrastructure.Persistence.Configurations;

internal sealed class StaffInvitationConfiguration : IEntityTypeConfiguration<StaffInvitation>
{
    public void Configure(EntityTypeBuilder<StaffInvitation> builder)
    {
        builder.ToTable("invitations", "identity");

        builder.HasKey(invitation => invitation.Id);
        builder.Property(invitation => invitation.Id).ValueGeneratedNever();
        builder.Property<uint>("Version").IsRowVersion();

        builder.Property(invitation => invitation.Email).HasMaxLength(StaffInvitation.EmailMaxLength).IsRequired();
        builder.Property(invitation => invitation.DisplayName).HasMaxLength(StaffInvitation.DisplayNameMaxLength).IsRequired();
        builder.Property(invitation => invitation.Role).HasConversion<string>().HasMaxLength(20);
        builder.Property(invitation => invitation.TokenHash).HasMaxLength(64).IsRequired();

        builder.HasIndex(invitation => invitation.TokenHash).IsUnique();
        builder.HasIndex(invitation => invitation.Email);
    }
}
