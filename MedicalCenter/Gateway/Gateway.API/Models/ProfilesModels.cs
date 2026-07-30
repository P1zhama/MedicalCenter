using System.ComponentModel.DataAnnotations;

namespace Gateway.Api.Models;

public record CreatePatientProfileWebRequest(
    [Required] string FirstName,
    [Required] string LastName,
    string? MiddleName,
    [Required] string PhoneNumber,
    [Required] string DateOfBirth,
    string? PhotoUrl
);

public record LinkExistingPatientWebRequest(
    [Required] string PatientId
);

public record MatchedProfileWebDto(
    string ProfileId,
    string FirstName,
    string LastName,
    string? MiddleName,
    string DateOfBirth
);

public record PatientProfileWebResponse(
    string ProfileId,
    bool IsMatched,
    MatchedProfileWebDto? MatchedProfile
);

public record CreatePatientByReceptionistWebRequest(
    [Required] string FirstName,
    [Required] string LastName,
    string? MiddleName,
    [Required] string DateOfBirth
);

public record CreateDoctorWebRequest(
    [Required] string FirstName,
    [Required] string LastName,
    string? MiddleName,
    [Required] string DateOfBirth,
    [Required][EmailAddress] string Email,
    [Required] string SpecializationId,
    [Required] string OfficeId,
    [Required] int CareerStartYear,
    string? Status,
    string? PhotoUrl
);

public record CreateReceptionistWebRequest(
    [Required] string FirstName,
    [Required] string LastName,
    string? MiddleName,
    [Required][EmailAddress] string Email,
    [Required] string OfficeId,
    string? PhotoUrl
);

public record CreatedProfileWebResponse(string ProfileId);
