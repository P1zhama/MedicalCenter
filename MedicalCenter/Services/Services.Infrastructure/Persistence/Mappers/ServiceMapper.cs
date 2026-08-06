using Common.Domain;
using Services.Domain.Enums;
using Services.Domain.Models;
using Services.Domain.ValueObjects;
using Services.Infrastructure.Persistence.Entities;

namespace Services.Infrastructure.Persistence.Mappers;

public static class ServiceMapper
{
    public static ServiceEntity ToEntity(this Service service) => new()
    {
        Id = service.Id,
        Name = service.Name,
        Price = service.Price.Amount,
        SpecializationId = service.SpecializationId,
        CategoryId = service.CategoryId,
        Status = service.Status.ToString(),
        Version = service.Version,
        CreatedBy = service.Audit.CreatedBy,
        CreatedAt = service.Audit.CreatedAt,
        UpdatedBy = service.Audit.UpdatedBy,
        UpdatedAt = service.Audit.UpdatedAt
    };

    public static Service ToDomain(this ServiceEntity entity)
        => Service.Restore(
            entity.Id,
            entity.Name,
            Price.Create(entity.Price),
            entity.SpecializationId,
            entity.CategoryId,
            Enum.Parse<ActivityStatus>(entity.Status),
            entity.Version,
            new AuditInfo(entity.CreatedBy, entity.CreatedAt, entity.UpdatedBy, entity.UpdatedAt));
}
