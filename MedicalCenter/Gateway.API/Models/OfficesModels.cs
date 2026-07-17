using System.ComponentModel.DataAnnotations;

namespace Gateway.API.Models;

public record CreateOfficeWebRequest(
    [Required] string City,
    [Required] string Street,
    [Required] string HouseNumber,
    string? OfficeNumber,
    [Required] string RegistryPhoneNumber,
    string? PhotoUrl,
    string? Status
);

public record UpdateOfficeWebRequest(
    [Required] string City,
    [Required] string Street,
    [Required] string HouseNumber,
    string? OfficeNumber,
    [Required] string RegistryPhoneNumber,
    string? PhotoUrl,
    [Required] string Status
);

public record ChangeOfficeStatusWebRequest(
    [Required] string Status
);

public record CreatedOfficeWebResponse(string OfficeId, string Message);

public record OfficeListItemWebResponse(
    string Id,
    string Address,
    string Status,
    string RegistryPhoneNumber
);

public record OfficeWebResponse(
    string Id,
    string PhotoUrl,
    string Address,
    string City,
    string Street,
    string HouseNumber,
    string OfficeNumber,
    string Status,
    string RegistryPhoneNumber
);
