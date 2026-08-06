using Common.Domain;
using Services.Domain.Enums;
using Services.Domain.Models;
using Services.Infrastructure.Persistence.Entities;

namespace Services.Infrastructure.Persistence.Mappers;

public static class SpecializationMapper
{
    public static SpecializationEntity ToEntity(this Specialization specialization) => new()
    {
        Id = specialization.Id,
        Name = specialization.Name,
        Status = specialization.Status.ToString(),
        Version = specialization.Version,
        CreatedBy = specialization.Audit.CreatedBy,
        CreatedAt = specialization.Audit.CreatedAt,
        UpdatedBy = specialization.Audit.UpdatedBy,
        UpdatedAt = specialization.Audit.UpdatedAt
    };

    public static Specialization ToDomain(this SpecializationEntity entity)
        => Specialization.Restore(
            entity.Id,
            entity.Name,
            Enum.Parse<ActivityStatus>(entity.Status),
            entity.Version,
            new AuditInfo(entity.CreatedBy, entity.CreatedAt, entity.UpdatedBy, entity.UpdatedAt));
}
