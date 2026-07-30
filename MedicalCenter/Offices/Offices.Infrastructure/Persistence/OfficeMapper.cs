using Common.Domain;
using Offices.Domain.Enums;
using Offices.Domain.Models;
using Offices.Domain.ValueObjects;

namespace Offices.Infrastructure.Persistence;

public static class OfficeMapper
{
    public static OfficeDocument ToDocument(this Office office) => new()
    {
        Id = office.Id,
        City = office.Address.City,
        Street = office.Address.Street,
        HouseNumber = office.Address.HouseNumber,
        OfficeNumber = office.Address.OfficeNumber,
        RegistryPhoneNumber = office.RegistryPhoneNumber,
        PhotoUrl = office.PhotoUrl,
        Status = office.Status.ToString(),
        Version = office.Version,
        CreatedBy = office.Audit.CreatedBy,
        CreatedAt = office.Audit.CreatedAt,
        UpdatedBy = office.Audit.UpdatedBy,
        UpdatedAt = office.Audit.UpdatedAt
    };

    public static Office ToDomain(this OfficeDocument document)
    {
        var address = Address.Create(document.City, document.Street, document.HouseNumber, document.OfficeNumber);

        return Office.Restore(
            document.Id,
            address,
            document.RegistryPhoneNumber,
            document.PhotoUrl,
            Enum.Parse<OfficeStatus>(document.Status),
            document.Version,
            new AuditInfo(document.CreatedBy, document.CreatedAt, document.UpdatedBy, document.UpdatedAt));
    }
}
