namespace Appointments.Application.Common.Dtos;

public record AvailableSlotDto(
    TimeOnly StartTime,
    IReadOnlyList<Guid> DoctorIds);

public record BusyIntervalDto(
    Guid DoctorId,
    TimeOnly StartTime,
    TimeOnly EndTime);

public record ServiceForAppointmentDto(
    Guid Id,
    string Name,
    Guid SpecializationId,
    int DurationMinutes,
    bool IsActive);

public record DoctorForAppointmentDto(
    Guid Id,
    Guid SpecializationId,
    Guid OfficeId,
    bool IsAtWork);
