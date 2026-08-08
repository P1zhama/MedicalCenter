namespace Services.Application.Common.Dtos;

public record SpecializationListItemDto(
    Guid Id,
    string Name,
    string Status);

public record SpecializationDto(
    Guid Id,
    string Name,
    string Status,
    IReadOnlyList<ServiceListItemDto> Services);

public record PublicSpecializationDto(
    Guid Id,
    string Name);
