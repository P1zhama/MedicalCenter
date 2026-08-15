using Appointments.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Appointments.Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<AppointmentEntity>
{
    public void Configure(EntityTypeBuilder<AppointmentEntity> builder)
    {
        builder.ToTable("appointments");
        builder.HasKey(appointment => appointment.Id);
        builder.Property(appointment => appointment.Id).HasColumnName("id");

        builder.Property(appointment => appointment.PatientId)
            .HasColumnName("patient_id")
            .IsRequired();

        builder.Property(appointment => appointment.DoctorId)
            .HasColumnName("doctor_id")
            .IsRequired();

        builder.Property(appointment => appointment.ServiceId)
            .HasColumnName("service_id")
            .IsRequired();

        builder.Property(appointment => appointment.OfficeId)
            .HasColumnName("office_id")
            .IsRequired();

        builder.Property(appointment => appointment.Date)
            .HasColumnName("date")
            .IsRequired();

        builder.Property(appointment => appointment.StartTime)
            .HasColumnName("start_time")
            .IsRequired();

        builder.Property(appointment => appointment.EndTime)
            .HasColumnName("end_time")
            .IsRequired();

        builder.Property(appointment => appointment.DurationMinutes)
            .HasColumnName("duration_minutes")
            .IsRequired();

        builder.Property(appointment => appointment.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(appointment => appointment.Version)
            .HasColumnName("version")
            .IsRequired()
            .IsConcurrencyToken();

        builder.Property(appointment => appointment.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        builder.Property(appointment => appointment.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(appointment => appointment.UpdatedBy)
            .HasColumnName("updated_by");

        builder.Property(appointment => appointment.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(appointment => new { appointment.DoctorId, appointment.Date, appointment.StartTime })
            .IsUnique()
            .HasFilter("status <> 'Cancelled'");

        builder.HasIndex(appointment => new { appointment.DoctorId, appointment.Date });
        builder.HasIndex(appointment => new { appointment.PatientId, appointment.Date });
    }
}
