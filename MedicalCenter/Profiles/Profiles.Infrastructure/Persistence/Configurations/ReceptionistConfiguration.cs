

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profiles.Domain;

namespace Profiles.Infrastructure.Persistence.Configurations;

public class ReceptionistConfiguration : IEntityTypeConfiguration<Receptionist>
{
    public void Configure(EntityTypeBuilder<Receptionist> builder)
    {
        builder.ToTable("Receptionists");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id");

        builder.Property(r => r.FirstName)
            .HasColumnName("first_name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.LastName)
            .HasColumnName("last_name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.MiddleName)
            .HasColumnName("middle_name")
            .HasMaxLength(100);

        builder.Property(r => r.AccountId)
            .HasColumnName("account_id")
            .IsRequired();

        builder.Property(r => r.OfficeId)
            .HasColumnName("office_id")
            .IsRequired();

        builder.Property(r => r.PhotoUrl)
            .HasColumnName("photo_url")
            .HasMaxLength(500);

        builder.HasIndex(r => r.AccountId).IsUnique();
        builder.HasIndex(r => r.OfficeId);
    }
}