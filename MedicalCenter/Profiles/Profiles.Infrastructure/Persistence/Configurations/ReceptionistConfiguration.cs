using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profiles.Infrastructure.Persistence.Entities;

namespace Profiles.Infrastructure.Persistence.Configurations;

public class ReceptionistConfiguration : IEntityTypeConfiguration<ReceptionistEntity>
{
    public void Configure(EntityTypeBuilder<ReceptionistEntity> builder)
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

        builder.Property(r => r.Version)
            .HasColumnName("version")
            .IsRequired()
            .IsConcurrencyToken();

        builder.Property(r => r.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(r => r.UpdatedBy)
            .HasColumnName("updated_by");

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(r => r.AccountId).IsUnique();
        builder.HasIndex(r => r.OfficeId);
    }
}
