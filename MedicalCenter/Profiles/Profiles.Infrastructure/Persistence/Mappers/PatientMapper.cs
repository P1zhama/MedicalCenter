using Common.Domain;
using Profiles.Domain;
using Profiles.Domain.ValueObjects;
using Profiles.Infrastructure.Persistence.Entities;

namespace Profiles.Infrastructure.Persistence.Mappers;

public static class PatientMapper
{
    public static PatientEntity ToEntity(this Patient patient) => new()
    {
        Id = patient.Id,
        AccountId = patient.AccountId,
        FirstName = patient.Name.FirstName,
        LastName = patient.Name.LastName,
        MiddleName = patient.Name.MiddleName,
        PhoneNumber = patient.PhoneNumber,
        DateOfBirth = patient.DateOfBirth,
        PhotoUrl = patient.PhotoUrl,
        Version = patient.Version,
        CreatedBy = patient.Audit.CreatedBy,
        CreatedAt = patient.Audit.CreatedAt,
        UpdatedBy = patient.Audit.UpdatedBy,
        UpdatedAt = patient.Audit.UpdatedAt
    };

    public static Patient ToDomain(this PatientEntity entity)
    {
        var nameResult = PersonName.Create(entity.FirstName, entity.LastName, entity.MiddleName);
        if (nameResult.IsError)
            throw new InvalidOperationException($"Patient {entity.Id} has an invalid name stored.");

        return Patient.Restore(
            entity.Id,
            entity.AccountId,
            nameResult.Value,
            entity.DateOfBirth,
            entity.PhoneNumber,
            entity.PhotoUrl,
            entity.Version,
            new AuditInfo(entity.CreatedBy, entity.CreatedAt, entity.UpdatedBy, entity.UpdatedAt));
    }
}
