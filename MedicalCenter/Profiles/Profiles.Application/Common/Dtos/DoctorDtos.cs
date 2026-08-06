namespace Profiles.Application.Common.Dtos;

public record DoctorFilter(
    string? Search,
    Guid? SpecializationId,
    Guid? OfficeId);

public record DoctorCardDto(
    Guid Id,
    string? PhotoUrl,
    string FirstName,
    string LastName,
    string? MiddleName,
    Guid SpecializationId,
    Guid OfficeId,
    int ExperienceYears);

public record DoctorListItemDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? MiddleName,
    DateOnly DateOfBirth,
    Guid SpecializationId,
    Guid OfficeId,
    string Status);

public record DoctorDto(
    Guid Id,
    string? PhotoUrl,
    string FirstName,
    string LastName,
    string? MiddleName,
    DateOnly DateOfBirth,
    Guid SpecializationId,
    Guid OfficeId,
    int CareerStartYear,
    int ExperienceYears,
    string Status);
