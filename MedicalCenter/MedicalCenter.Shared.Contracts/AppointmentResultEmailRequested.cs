using System;

namespace MedicalCenter.Shared.Contracts;

public record AppointmentResultEmailRequested(
    Guid PatientProfileId,
    string FileName,
    byte[] Content
);
