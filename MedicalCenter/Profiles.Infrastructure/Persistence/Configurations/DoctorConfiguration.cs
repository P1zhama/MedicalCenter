
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profiles.Domain;

namespace Profiles.Infrastructure.Persistence.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("Doctors");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).HasColumnName("id");

        builder.Property(d => d.FirstName)
            .HasColumnName("first_name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.LastName)
            .HasColumnName("last_name")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.MiddleName)
            .HasColumnName("middle_name")
            .HasMaxLength(100);

        builder.Property(d => d.DateOfBirth)
            .HasColumnName("date_of_birth")
            .IsRequired();

        builder.Property(d => d.AccountId)
            .HasColumnName("account_id")
            .IsRequired();

        builder.Property(d => d.SpecializationId)
            .HasColumnName("specialization_id")
            .IsRequired();

        builder.Property(d => d.OfficeId)
            .HasColumnName("office_id")
            .IsRequired();

        builder.Property(d => d.CareerStartYear)
            .HasColumnName("career_start_year")
            .IsRequired();

        builder.Property(d => d.Status)
            .HasColumnName("status")
            .IsRequired();

        builder.Property(d => d.PhotoUrl)
            .HasColumnName("photo_url")
            .HasMaxLength(500);

        builder.HasIndex(d => d.AccountId).IsUnique();
        builder.HasIndex(d => d.OfficeId);
        builder.HasIndex(d => d.SpecializationId);
    }
}
