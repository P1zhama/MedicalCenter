namespace Documents.Application.Common.Dtos;

public record AppointmentResultDocumentDto(
    Guid AppointmentId,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string PatientFullName,
    DateOnly PatientDateOfBirth,
    string DoctorFullName,
    string SpecializationName,
    string ServiceName,
    string? Complaints,
    string? Conclusion,
    string? Diagnosis,
    string? Recommendations)
{
    public bool HasResult => !string.IsNullOrWhiteSpace(Conclusion);
}

public record GeneratedFileDto(byte[] Content, string ContentType, string FileName);
