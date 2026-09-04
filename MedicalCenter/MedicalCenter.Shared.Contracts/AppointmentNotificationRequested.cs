using System;

namespace MedicalCenter.Shared.Contracts;

public record AppointmentNotificationRequested(
    Guid PatientProfileId,
    string Kind,
    DateTime AppointmentStart,
    string PatientFullName,
    string DoctorFullName,
    string ServiceName
);
