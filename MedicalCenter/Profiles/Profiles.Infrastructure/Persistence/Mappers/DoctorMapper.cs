using Common.Domain;
using Profiles.Domain;
using Profiles.Domain.Enums;
using Profiles.Domain.ValueObjects;
using Profiles.Infrastructure.Persistence.Entities;

namespace Profiles.Infrastructure.Persistence.Mappers;

public static class DoctorMapper
{
    public static DoctorEntity ToEntity(this Doctor doctor) => new()
    {
        Id = doctor.Id,
        FirstName = doctor.Name.FirstName,
        LastName = doctor.Name.LastName,
        MiddleName = doctor.Name.MiddleName,
        DateOfBirth = doctor.DateOfBirth,
        AccountId = doctor.AccountId,
        SpecializationId = doctor.SpecializationId,
        OfficeId = doctor.OfficeId,
        CareerStartYear = doctor.CareerStartYear,
        Status = doctor.Status.ToString(),
        PhotoUrl = doctor.PhotoUrl,
        Version = doctor.Version,
        CreatedBy = doctor.Audit.CreatedBy,
        CreatedAt = doctor.Audit.CreatedAt,
        UpdatedBy = doctor.Audit.UpdatedBy,
        UpdatedAt = doctor.Audit.UpdatedAt
    };

    public static Doctor ToDomain(this DoctorEntity entity)
    {
        var nameResult = PersonName.Create(entity.FirstName, entity.LastName, entity.MiddleName);
        if (nameResult.IsError)
            throw new InvalidOperationException($"Doctor {entity.Id} has an invalid name stored.");

        return Doctor.Restore(
            entity.Id,
            entity.AccountId,
            nameResult.Value,
            entity.DateOfBirth,
            entity.SpecializationId,
            entity.OfficeId,
            entity.CareerStartYear,
            Enum.Parse<DoctorStatus>(entity.Status),
            entity.PhotoUrl,
            entity.Version,
            new AuditInfo(entity.CreatedBy, entity.CreatedAt, entity.UpdatedBy, entity.UpdatedAt));
    }
}
