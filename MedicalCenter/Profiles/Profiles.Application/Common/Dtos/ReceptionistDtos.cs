namespace Profiles.Application.Common.Dtos;

public record ReceptionistListItemDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? MiddleName,
    Guid OfficeId,
    string Status);

public record ReceptionistDto(
    Guid Id,
    string? PhotoUrl,
    string FirstName,
    string LastName,
    string? MiddleName,
    Guid OfficeId,
    string Status);
