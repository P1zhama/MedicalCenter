namespace Profiles.Application.Common.Dtos;

public record PatientListItemDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? MiddleName,
    string? PhoneNumber);

public record PatientDto(
    Guid Id,
    string? PhotoUrl,
    string FirstName,
    string LastName,
    string? MiddleName,
    string? PhoneNumber,
    DateOnly DateOfBirth,
    bool IsLinkedToAccount);
