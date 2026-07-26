using Offices.Application.Common.Dtos;
using Offices.Domain;

namespace Offices.Application.Common.Mappings;

public static class OfficeMappings
{
    public static OfficeListItemDto ToListItem(this Office office) => new(
        office.Id,
        office.Address.Format(),
        office.Status.ToString(),
        office.RegistryPhoneNumber);

    public static OfficeDto ToDto(this Office office) => new(
        office.Id,
        office.PhotoUrl,
        office.Address.Format(),
        office.Address.City,
        office.Address.Street,
        office.Address.HouseNumber,
        office.Address.OfficeNumber,
        office.Status.ToString(),
        office.RegistryPhoneNumber);
}
