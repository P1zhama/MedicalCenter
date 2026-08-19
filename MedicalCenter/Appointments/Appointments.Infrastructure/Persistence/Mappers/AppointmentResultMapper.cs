using Appointments.Domain;
using Appointments.Infrastructure.Persistence.Entities;
using Common.Domain;

namespace Appointments.Infrastructure.Persistence.Mappers;

public static class AppointmentResultMapper
{
    public static AppointmentResultEntity ToEntity(this AppointmentResult result) => new()
    {
        Id = result.Id,
        AppointmentId = result.AppointmentId,
        Complaints = result.Complaints,
        Conclusion = result.Conclusion,
        Recommendations = result.Recommendations,
        Diagnosis = result.Diagnosis,
        Version = result.Version,
        CreatedBy = result.Audit.CreatedBy,
        CreatedAt = result.Audit.CreatedAt,
        UpdatedBy = result.Audit.UpdatedBy,
        UpdatedAt = result.Audit.UpdatedAt
    };

    public static AppointmentResult ToDomain(this AppointmentResultEntity entity)
        => AppointmentResult.Restore(
            entity.Id,
            entity.AppointmentId,
            entity.Complaints,
            entity.Conclusion,
            entity.Recommendations,
            entity.Diagnosis,
            entity.Version,
            new AuditInfo(entity.CreatedBy, entity.CreatedAt, entity.UpdatedBy, entity.UpdatedAt));
}
