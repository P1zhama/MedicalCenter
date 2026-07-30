namespace Gateway.Api.Models;

public record CreateOfficeWebRequest(
    string City,
    string Street,
    string HouseNumber,
    string OfficeNumber,
    string RegistryPhoneNumber,
    string PhotoUrl,
    string Status
);

public record UpdateOfficeWebRequest(
    string City,
    string Street,
    string HouseNumber,
    string OfficeNumber,
    string RegistryPhoneNumber,
    string PhotoUrl,
    string Status
);

public record ChangeOfficeStatusWebRequest(
    string Status
);

public record CreatedOfficeWebResponse(string OfficeId);

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
