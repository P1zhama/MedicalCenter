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

public record UpdatePatientWebRequest(
    string FirstName,
    string LastName,
    string MiddleName,
    string PhoneNumber,
    string DateOfBirth,
    string PhotoUrl
);

public record PatientListItemWebResponse(
    string Id,
    string FirstName,
    string LastName,
    string MiddleName,
    string PhoneNumber
);

public record PatientWebResponse(
    string Id,
    string PhotoUrl,
    string FirstName,
    string LastName,
    string MiddleName,
    string PhoneNumber,
    string DateOfBirth,
    bool IsLinkedToAccount
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

public record UpdateDoctorWebRequest(
    string FirstName,
    string LastName,
    string MiddleName,
    string DateOfBirth,
    string SpecializationId,
    string OfficeId,
    int CareerStartYear,
    string Status,
    string PhotoUrl
);

public record UpdateMyDoctorProfileWebRequest(
    string FirstName,
    string LastName,
    string MiddleName,
    string DateOfBirth,
    string SpecializationId,
    string OfficeId,
    int CareerStartYear,
    string PhotoUrl
);

public record DoctorCardWebResponse(
    string Id,
    string PhotoUrl,
    string FirstName,
    string LastName,
    string MiddleName,
    string SpecializationId,
    string OfficeId,
    int ExperienceYears
);

public record DoctorListItemWebResponse(
    string Id,
    string FirstName,
    string LastName,
    string MiddleName,
    string DateOfBirth,
    string SpecializationId,
    string OfficeId,
    string Status
);

public record DoctorWebResponse(
    string Id,
    string PhotoUrl,
    string FirstName,
    string LastName,
    string MiddleName,
    string DateOfBirth,
    string SpecializationId,
    string OfficeId,
    int CareerStartYear,
    int ExperienceYears,
    string Status
);

public record CreateReceptionistWebRequest(
    string FirstName,
    string LastName,
    string MiddleName,
    string Email,
    string OfficeId,
    string PhotoUrl
);

public record UpdateReceptionistWebRequest(
    string FirstName,
    string LastName,
    string MiddleName,
    string OfficeId,
    string Status,
    string PhotoUrl
);

public record UpdateMyReceptionistProfileWebRequest(
    string FirstName,
    string LastName,
    string MiddleName,
    string OfficeId,
    string PhotoUrl
);

public record ReceptionistListItemWebResponse(
    string Id,
    string FirstName,
    string LastName,
    string MiddleName,
    string OfficeId,
    string Status
);

public record ReceptionistWebResponse(
    string Id,
    string PhotoUrl,
    string FirstName,
    string LastName,
    string MiddleName,
    string OfficeId,
    string Status
);

public record CreatedProfileWebResponse(string ProfileId);
