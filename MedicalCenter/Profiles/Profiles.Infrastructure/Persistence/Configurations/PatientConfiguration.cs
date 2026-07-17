using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profiles.Domain;

namespace Profiles.Infrastructure.Persistence.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        builder.Property(p => p.AccountId)
            .HasColumnName("account_id");

        builder.Property(p => p.FirstName)
            .HasColumnName("first_name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.LastName)
            .HasColumnName("last_name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.MiddleName)
            .HasColumnName("middle_name")
            .HasMaxLength(100);

        builder.Property(p => p.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(20);

        builder.Property(p => p.DateOfBirth)
            .HasColumnName("date_of_birth")
            .IsRequired();

        builder.Property(p => p.PhotoUrl)
            .HasColumnName("photo_url")
            .HasMaxLength(500);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Ignore(p => p.IsLinkedToAccount);

        builder.HasIndex(p => new { p.LastName, p.FirstName, p.DateOfBirth });
        builder.HasIndex(p => p.PhoneNumber).IsUnique().HasFilter("[phone_number] IS NOT NULL");
        builder.HasIndex(p => p.AccountId).IsUnique().HasFilter("[account_id] IS NOT NULL");
    }
}
