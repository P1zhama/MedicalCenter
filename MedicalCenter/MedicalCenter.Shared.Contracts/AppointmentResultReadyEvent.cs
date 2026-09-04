using System;

namespace MedicalCenter.Shared.Contracts;

public record AppointmentResultReadyEvent(
    Guid AppointmentId,
    Guid PatientProfileId,
    DateTime AppointmentStart,
    DateTime AppointmentEnd,
    string PatientFullName,
    DateTime PatientDateOfBirth,
    string DoctorFullName,
    string SpecializationName,
    string ServiceName,
    string Complaints,
    string Conclusion,
    string? Diagnosis,
    string Recommendations
);
