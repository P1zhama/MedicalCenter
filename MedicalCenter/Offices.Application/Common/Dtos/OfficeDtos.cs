
namespace Offices.Application.Common.Dtos;

public record OfficeListItemDto(
    Guid Id,
    string Address,
    string Status,
    string RegistryPhoneNumber);

public record OfficeDto(
    Guid Id,
    string? PhotoUrl,
    string Address,
    string City,
    string Street,
    string HouseNumber,
    string? OfficeNumber,
    string Status,
    string RegistryPhoneNumber);
