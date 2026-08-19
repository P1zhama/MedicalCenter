namespace Appointments.Application.Common.Dtos;

public record AppointmentResultRowDto(
    Guid Id,
    Guid AppointmentId,
    string Complaints,
    string Conclusion,
    string Recommendations,
    string? Diagnosis);

public record AppointmentResultDto(
    Guid? Id,
    Guid AppointmentId,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    Guid PatientId,
    string PatientFirstName,
    string PatientLastName,
    string? PatientMiddleName,
    DateOnly PatientDateOfBirth,
    Guid DoctorId,
    string DoctorFirstName,
    string DoctorLastName,
    string? DoctorMiddleName,
    string SpecializationName,
    Guid ServiceId,
    string ServiceName,
    string? Complaints,
    string? Conclusion,
    string? Recommendations,
    string? Diagnosis);
