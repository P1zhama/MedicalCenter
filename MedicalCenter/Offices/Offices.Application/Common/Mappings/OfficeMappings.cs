using Offices.Application.Common.Dtos;
using Offices.Domain;

namespace Offices.Application.Common.Mappings;

public static class OfficeMappings
{
    public static OfficeListItemDto ToListItem(this Office office) => new(
        office.Id,
        office.FormatAddress(),
        office.Status.ToString(),
        office.RegistryPhoneNumber);

    public static OfficeDto ToDto(this Office office) => new(
        office.Id,
        office.PhotoUrl,
        office.FormatAddress(),
        office.City,
        office.Street,
        office.HouseNumber,
        office.OfficeNumber,
        office.Status.ToString(),
        office.RegistryPhoneNumber);
}
