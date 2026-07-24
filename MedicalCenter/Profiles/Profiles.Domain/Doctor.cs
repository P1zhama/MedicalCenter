using Common.Domain;
using ErrorOr;
using Profiles.Domain.Enums;
using Profiles.Domain.ValueObjects;

namespace Profiles.Domain;

public sealed class Doctor : AggregateRoot<Guid>
{
    private Doctor(
        Guid id,
        Guid accountId,
        PersonName name,
        DateOnly dateOfBirth,
        Guid specializationId,
        Guid officeId,
        int careerStartYear,
        DoctorStatus status,
        string? photoUrl,
        long version,
        AuditInfo audit)
        : base(id, version, audit)
    {
        AccountId = accountId;
        Name = name;
        DateOfBirth = dateOfBirth;
        SpecializationId = specializationId;
        OfficeId = officeId;
        CareerStartYear = careerStartYear;
        Status = status;
        PhotoUrl = photoUrl;
    }

    public Guid AccountId { get; private set; }

    public PersonName Name { get; private set; }

    public DateOnly DateOfBirth { get; private set; }

    public Guid SpecializationId { get; private set; }

    public Guid OfficeId { get; private set; }

    public int CareerStartYear { get; private set; }

    public DoctorStatus Status { get; private set; }

    public string? PhotoUrl { get; private set; }

    public static ErrorOr<Doctor> Create(
        Guid id,
        Guid accountId,
        PersonName name,
        DateOnly dateOfBirth,
        Guid specializationId,
        Guid officeId,
        int careerStartYear,
        DoctorStatus status,
        string? photoUrl,
        Guid createdBy,
        DateTimeOffset createdAt)
    {
        if (accountId == Guid.Empty)
            return Error.Validation("Doctor.AccountId", "Account id must not be empty.");

        if (specializationId == Guid.Empty)
            return Error.Validation("Doctor.SpecializationId", "Please, choose the specialisation");

        if (officeId == Guid.Empty)
            return Error.Validation("Doctor.OfficeId", "Please, choose the office");

        var today = DateOnly.FromDateTime(createdAt.UtcDateTime);

        if (dateOfBirth > today)
            return Error.Validation("Doctor.DateOfBirth", "Date of birth must not be in the future.");

        if (careerStartYear < 1900 || careerStartYear > today.Year)
            return Error.Validation("Doctor.CareerStartYear", "Career start year is out of range.");

        return new Doctor(
            id,
            accountId,
            name,
            dateOfBirth,
            specializationId,
            officeId,
            careerStartYear,
            status,
            photoUrl,
            version: 1,
            new AuditInfo(createdBy, createdAt, null, null));
    }

    public int ExperienceYears(int currentYear) => currentYear - CareerStartYear + 1;

    public static Doctor Restore(
        Guid id,
        Guid accountId,
        PersonName name,
        DateOnly dateOfBirth,
        Guid specializationId,
        Guid officeId,
        int careerStartYear,
        DoctorStatus status,
        string? photoUrl,
        long version,
        AuditInfo audit)
        => new(
            id,
            accountId,
            name,
            dateOfBirth,
            specializationId,
            officeId,
            careerStartYear,
            status,
            photoUrl,
            version,
            audit);
}
