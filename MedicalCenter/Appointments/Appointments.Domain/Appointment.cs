using Appointments.Domain.Enums;
using Common.Domain;
using Common.Domain.Exceptions;

namespace Appointments.Domain;

public sealed class Appointment : AggregateRoot<Guid>
{
    private Appointment(
        Guid id,
        Guid patientId,
        Guid doctorId,
        Guid serviceId,
        Guid officeId,
        DateOnly date,
        TimeOnly startTime,
        int durationMinutes,
        AppointmentStatus status,
        long version,
        AuditInfo audit)
        : base(id, version, audit)
    {
        PatientId = patientId;
        DoctorId = doctorId;
        ServiceId = serviceId;
        OfficeId = officeId;
        Date = date;
        StartTime = startTime;
        DurationMinutes = durationMinutes;
        Status = status;
    }

    public Guid PatientId { get; private set; }

    public Guid DoctorId { get; private set; }

    public Guid ServiceId { get; private set; }

    public Guid OfficeId { get; private set; }

    public DateOnly Date { get; private set; }

    public TimeOnly StartTime { get; private set; }

    public int DurationMinutes { get; private set; }

    public AppointmentStatus Status { get; private set; }

    public TimeOnly EndTime => StartTime.AddMinutes(DurationMinutes);

    public bool IsApproved => Status == AppointmentStatus.Approved;

    public bool IsCancelled => Status == AppointmentStatus.Cancelled;

    public static Appointment Create(
        Guid id,
        Guid patientId,
        Guid doctorId,
        Guid serviceId,
        Guid officeId,
        DateOnly date,
        TimeOnly startTime,
        int durationMinutes,
        Guid createdBy,
        DateTimeOffset createdAt)
    {
        EnsureNotEmpty(id, nameof(id));
        EnsureNotEmpty(patientId, nameof(patientId));
        EnsureNotEmpty(doctorId, nameof(doctorId));
        EnsureNotEmpty(serviceId, nameof(serviceId));
        EnsureNotEmpty(officeId, nameof(officeId));

        if (durationMinutes <= 0)
            throw new DomainException("Appointment duration must be greater than zero.");

        return new Appointment(
            id,
            patientId,
            doctorId,
            serviceId,
            officeId,
            date,
            startTime,
            durationMinutes,
            AppointmentStatus.NotApproved,
            version: 1,
            new AuditInfo(createdBy, createdAt, null, null));
    }

    public static Appointment Restore(
        Guid id,
        Guid patientId,
        Guid doctorId,
        Guid serviceId,
        Guid officeId,
        DateOnly date,
        TimeOnly startTime,
        int durationMinutes,
        AppointmentStatus status,
        long version,
        AuditInfo audit)
        => new(
            id,
            patientId,
            doctorId,
            serviceId,
            officeId,
            date,
            startTime,
            durationMinutes,
            status,
            version,
            audit);

    public void Approve(Guid updatedBy, DateTimeOffset now)
    {
        if (IsCancelled)
            throw new DomainException("Cancelled appointment cannot be approved.");

        if (IsApproved)
            throw new DomainException("Appointment is already approved.");

        Status = AppointmentStatus.Approved;

        MarkUpdated(updatedBy, now);
    }

    public void Reschedule(
        Guid doctorId,
        DateOnly date,
        TimeOnly startTime,
        int durationMinutes,
        Guid updatedBy,
        DateTimeOffset now)
    {
        EnsureNotEmpty(doctorId, nameof(doctorId));

        if (durationMinutes <= 0)
            throw new DomainException("Appointment duration must be greater than zero.");

        if (IsCancelled)
            throw new DomainException("Cancelled appointment cannot be rescheduled.");

        if (IsApproved)
            throw new DomainException("Approved appointment cannot be rescheduled.");

        DoctorId = doctorId;
        Date = date;
        StartTime = startTime;
        DurationMinutes = durationMinutes;

        MarkUpdated(updatedBy, now);
    }

    public void Cancel(Guid updatedBy, DateTimeOffset now)
    {
        if (IsCancelled)
            throw new DomainException("Appointment is already cancelled.");

        Status = AppointmentStatus.Cancelled;

        MarkUpdated(updatedBy, now);
    }

    private void MarkUpdated(Guid updatedBy, DateTimeOffset now)
    {
        Audit = Audit.WithUpdate(updatedBy, now);
        Version++;
    }

    private static void EnsureNotEmpty(Guid value, string name)
    {
        if (value == Guid.Empty)
            throw new DomainException($"Appointment {name} must not be empty.");
    }
}
