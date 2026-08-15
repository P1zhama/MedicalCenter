namespace Services.Application.Common.Dtos;

public record ServiceListItemDto(
    Guid Id,
    string Name,
    decimal Price,
    string Status,
    Guid CategoryId,
    string CategoryName);

public record ServiceDto(
    Guid Id,
    string Name,
    decimal Price,
    string Status,
    Guid CategoryId,
    string CategoryName,
    Guid SpecializationId,
    string SpecializationName);

public record ServiceSummaryDto(
    Guid Id,
    string Name);

public record ServiceForAppointmentDto(
    Guid Id,
    string Name,
    decimal Price,
    Guid SpecializationId,
    Guid CategoryId,
    int TimeSlotMinutes,
    bool IsActive);

public record ServiceCategoryDto(
    Guid Id,
    string Name,
    int TimeSlotMinutes);
