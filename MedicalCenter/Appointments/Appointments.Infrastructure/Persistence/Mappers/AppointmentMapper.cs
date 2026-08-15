using Appointments.Domain;
using Appointments.Domain.Enums;
using Appointments.Infrastructure.Persistence.Entities;
using Common.Domain;

namespace Appointments.Infrastructure.Persistence.Mappers;

public static class AppointmentMapper
{
    public static AppointmentEntity ToEntity(this Appointment appointment) => new()
    {
        Id = appointment.Id,
        PatientId = appointment.PatientId,
        DoctorId = appointment.DoctorId,
        ServiceId = appointment.ServiceId,
        OfficeId = appointment.OfficeId,
        Date = appointment.Date,
        StartTime = appointment.StartTime,
        EndTime = appointment.EndTime,
        DurationMinutes = appointment.DurationMinutes,
        Status = appointment.Status.ToString(),
        Version = appointment.Version,
        CreatedBy = appointment.Audit.CreatedBy,
        CreatedAt = appointment.Audit.CreatedAt,
        UpdatedBy = appointment.Audit.UpdatedBy,
        UpdatedAt = appointment.Audit.UpdatedAt
    };

    public static Appointment ToDomain(this AppointmentEntity entity)
        => Appointment.Restore(
            entity.Id,
            entity.PatientId,
            entity.DoctorId,
            entity.ServiceId,
            entity.OfficeId,
            entity.Date,
            entity.StartTime,
            entity.DurationMinutes,
            Enum.Parse<AppointmentStatus>(entity.Status),
            entity.Version,
            new AuditInfo(entity.CreatedBy, entity.CreatedAt, entity.UpdatedBy, entity.UpdatedAt));
}
