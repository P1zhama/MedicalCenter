namespace Gateway.Api.Models;

public record CreatePatientProfileWebRequest(
    string FirstName,
    string LastName,
    string MiddleName,
    string PhoneNumber,
    string DateOfBirth,
    string PhotoUrl
);

public record LinkExistingPatientWebRequest(
    string PatientId
);

public record MatchedProfileWebDto(
    string ProfileId,
    string FirstName,
    string LastName,
    string MiddleName,
    string DateOfBirth
);

public record PatientProfileWebResponse(
    string ProfileId,
    bool IsMatched,
    MatchedProfileWebDto? MatchedProfile
);

public record CreatePatientByReceptionistWebRequest(
    string FirstName,
    string LastName,
    string MiddleName,
    string DateOfBirth
);

public record CreateDoctorWebRequest(
    string FirstName,
    string LastName,
    string MiddleName,
    string DateOfBirth,
    string Email,
    string SpecializationId,
    string OfficeId,
    int CareerStartYear,
    string Status,
    string PhotoUrl
);

public record CreateReceptionistWebRequest(
    string FirstName,
    string LastName,
    string MiddleName,
    string Email,
    string OfficeId,
    string PhotoUrl
);

public record CreatedProfileWebResponse(string ProfileId);
