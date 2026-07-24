using Common.Domain;
using Profiles.Domain;
using Profiles.Domain.ValueObjects;
using Profiles.Infrastructure.Persistence.Entities;

namespace Profiles.Infrastructure.Persistence.Mappers;

public static class ReceptionistMapper
{
    public static ReceptionistEntity ToEntity(this Receptionist receptionist) => new()
    {
        Id = receptionist.Id,
        FirstName = receptionist.Name.FirstName,
        LastName = receptionist.Name.LastName,
        MiddleName = receptionist.Name.MiddleName,
        AccountId = receptionist.AccountId,
        OfficeId = receptionist.OfficeId,
        PhotoUrl = receptionist.PhotoUrl,
        Version = receptionist.Version,
        CreatedBy = receptionist.Audit.CreatedBy,
        CreatedAt = receptionist.Audit.CreatedAt,
        UpdatedBy = receptionist.Audit.UpdatedBy,
        UpdatedAt = receptionist.Audit.UpdatedAt
    };

    public static Receptionist ToDomain(this ReceptionistEntity entity)
    {
        var nameResult = PersonName.Create(entity.FirstName, entity.LastName, entity.MiddleName);
        if (nameResult.IsError)
            throw new InvalidOperationException($"Receptionist {entity.Id} has an invalid name stored.");

        return Receptionist.Restore(
            entity.Id,
            entity.AccountId,
            nameResult.Value,
            entity.OfficeId,
            entity.PhotoUrl,
            entity.Version,
            new AuditInfo(entity.CreatedBy, entity.CreatedAt, entity.UpdatedBy, entity.UpdatedAt));
    }
}
