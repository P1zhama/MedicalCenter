using Common.Domain;
using Common.Domain.Exceptions;

namespace Appointments.Domain;

public sealed class AppointmentResult : AggregateRoot<Guid>
{
    private const int TextMaxLength = 2000;
    private const int DiagnosisMaxLength = 500;

    private AppointmentResult(
        Guid id,
        Guid appointmentId,
        string complaints,
        string conclusion,
        string recommendations,
        string? diagnosis,
        long version,
        AuditInfo audit)
        : base(id, version, audit)
    {
        AppointmentId = appointmentId;
        Complaints = complaints;
        Conclusion = conclusion;
        Recommendations = recommendations;
        Diagnosis = diagnosis;
    }

    public Guid AppointmentId { get; private set; }

    public string Complaints { get; private set; }

    public string Conclusion { get; private set; }

    public string Recommendations { get; private set; }

    public string? Diagnosis { get; private set; }

    public static AppointmentResult Create(
        Guid id,
        Guid appointmentId,
        string complaints,
        string conclusion,
        string recommendations,
        string? diagnosis,
        Guid createdBy,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
            throw new DomainException("Appointment result id must not be empty.");

        if (appointmentId == Guid.Empty)
            throw new DomainException("Appointment result appointment id must not be empty.");

        EnsureTexts(complaints, conclusion, recommendations, diagnosis);

        return new AppointmentResult(
            id,
            appointmentId,
            complaints,
            conclusion,
            recommendations,
            diagnosis,
            version: 1,
            new AuditInfo(createdBy, createdAt, null, null));
    }

    public static AppointmentResult Restore(
        Guid id,
        Guid appointmentId,
        string complaints,
        string conclusion,
        string recommendations,
        string? diagnosis,
        long version,
        AuditInfo audit)
        => new(id, appointmentId, complaints, conclusion, recommendations, diagnosis, version, audit);

    public void Update(
        string complaints,
        string conclusion,
        string recommendations,
        string? diagnosis,
        Guid updatedBy,
        DateTimeOffset now)
    {
        EnsureTexts(complaints, conclusion, recommendations, diagnosis);

        Complaints = complaints;
        Conclusion = conclusion;
        Recommendations = recommendations;
        Diagnosis = diagnosis;

        Audit = Audit.WithUpdate(updatedBy, now);
        Version++;
    }

    private static void EnsureTexts(string complaints, string conclusion, string recommendations, string? diagnosis)
    {
        EnsureRequiredText(complaints, nameof(complaints));
        EnsureRequiredText(conclusion, nameof(conclusion));
        EnsureRequiredText(recommendations, nameof(recommendations));

        if (diagnosis is not null && diagnosis.Length > DiagnosisMaxLength)
            throw new DomainException($"Appointment result diagnosis must not exceed {DiagnosisMaxLength} characters.");
    }

    private static void EnsureRequiredText(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"Appointment result {name} must not be empty.");

        if (value.Length > TextMaxLength)
            throw new DomainException($"Appointment result {name} must not exceed {TextMaxLength} characters.");
    }
}
