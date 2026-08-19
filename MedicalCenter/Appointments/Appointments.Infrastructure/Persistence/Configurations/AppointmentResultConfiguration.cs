using Appointments.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Appointments.Infrastructure.Persistence.Configurations;

public class AppointmentResultConfiguration : IEntityTypeConfiguration<AppointmentResultEntity>
{
    public void Configure(EntityTypeBuilder<AppointmentResultEntity> builder)
    {
        builder.ToTable("appointment_results");
        builder.HasKey(result => result.Id);
        builder.Property(result => result.Id).HasColumnName("id");

        builder.Property(result => result.AppointmentId)
            .HasColumnName("appointment_id")
            .IsRequired();

        builder.Property(result => result.Complaints)
            .HasColumnName("complaints")
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(result => result.Conclusion)
            .HasColumnName("conclusion")
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(result => result.Recommendations)
            .HasColumnName("recommendations")
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(result => result.Diagnosis)
            .HasColumnName("diagnosis")
            .HasMaxLength(500);

        builder.Property(result => result.Version)
            .HasColumnName("version")
            .IsRequired()
            .IsConcurrencyToken();

        builder.Property(result => result.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        builder.Property(result => result.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(result => result.UpdatedBy)
            .HasColumnName("updated_by");

        builder.Property(result => result.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(result => result.AppointmentId).IsUnique();
    }
}
