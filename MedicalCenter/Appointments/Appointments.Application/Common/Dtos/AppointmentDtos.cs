namespace Appointments.Application.Common.Dtos;

public record AppointmentFilter(
    DateOnly Date,
    Guid? DoctorId,
    Guid? ServiceId,
    Guid? OfficeId,
    string? Status);

public record AppointmentRowDto(
    Guid Id,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    Guid DoctorId,
    Guid PatientId,
    Guid ServiceId,
    Guid OfficeId,
    string Status);

public record DoctorSummaryDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? MiddleName);

public record PatientSummaryDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? MiddleName,
    string? PhoneNumber,
    DateOnly DateOfBirth);

public record ServiceSummaryDto(
    Guid Id,
    string Name,
    string SpecializationName);

public record AppointmentListItemDto(
    Guid Id,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    Guid DoctorId,
    string DoctorFirstName,
    string DoctorLastName,
    string? DoctorMiddleName,
    Guid PatientId,
    string? PatientFirstName,
    string? PatientLastName,
    string? PatientMiddleName,
    string? PatientPhoneNumber,
    Guid ServiceId,
    string ServiceName,
    string Status);

public record AppointmentReminderDto(
    Guid Id,
    Guid PatientId,
    string PatientFullName,
    Guid DoctorId,
    Guid ServiceId,
    DateOnly Date,
    TimeOnly StartTime,
    long Version);
