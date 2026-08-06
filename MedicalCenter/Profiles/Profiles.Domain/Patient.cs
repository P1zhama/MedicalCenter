using Common.Domain;
using ErrorOr;
using Profiles.Domain.ValueObjects;

namespace Profiles.Domain;

public sealed class Patient : AggregateRoot<Guid>
{
    private Patient(
        Guid id,
        Guid? accountId,
        PersonName name,
        DateOnly dateOfBirth,
        string? phoneNumber,
        string? photoUrl,
        long version,
        AuditInfo audit)
        : base(id, version, audit)
    {
        AccountId = accountId;
        Name = name;
        DateOfBirth = dateOfBirth;
        PhoneNumber = phoneNumber;
        PhotoUrl = photoUrl;
    }

    public Guid? AccountId { get; private set; }

    public PersonName Name { get; private set; }

    public DateOnly DateOfBirth { get; private set; }

    public string? PhoneNumber { get; private set; }

    public string? PhotoUrl { get; private set; }

    public bool IsLinkedToAccount => AccountId.HasValue;

    public static ErrorOr<Patient> Create(
        Guid id,
        Guid? accountId,
        PersonName name,
        DateOnly dateOfBirth,
        string? phoneNumber,
        string? photoUrl,
        Guid createdBy,
        DateTimeOffset createdAt)
    {
        var today = DateOnly.FromDateTime(createdAt.UtcDateTime);

        if (dateOfBirth > today)
            return Error.Validation("Patient.DateOfBirth", "Date of birth must not be in the future.");

        return new Patient(
            id,
            accountId,
            name,
            dateOfBirth,
            phoneNumber,
            photoUrl,
            version: 1,
            new AuditInfo(createdBy, createdAt, null, null));
    }

    public ErrorOr<Success> Update(
        PersonName name,
        DateOnly dateOfBirth,
        string? phoneNumber,
        string? photoUrl,
        Guid updatedBy,
        DateTimeOffset at)
    {
        var today = DateOnly.FromDateTime(at.UtcDateTime);

        if (dateOfBirth > today)
            return Error.Validation("Patient.DateOfBirth", "Date of birth must not be in the future.");

        Name = name;
        DateOfBirth = dateOfBirth;
        PhoneNumber = phoneNumber;
        PhotoUrl = photoUrl;
        Audit = Audit.WithUpdate(updatedBy, at);
        Version++;

        return Result.Success;
    }

    public ErrorOr<Success> LinkToAccount(Guid accountId, DateTimeOffset at)
    {
        if (accountId == Guid.Empty)
            return Error.Validation("Patient.AccountId", "Account id must not be empty.");
        
        if (IsLinkedToAccount)
            return Error.Conflict("Patient.AlreadyLinked", "Patient is already linked to an account.");

        AccountId = accountId;
        Audit = Audit.WithUpdate(accountId, at);
        Version++;

        return Result.Success;
    }

    public static Patient Restore(
        Guid id,
        Guid? accountId,
        PersonName name,
        DateOnly dateOfBirth,
        string? phoneNumber,
        string? photoUrl,
        long version,
        AuditInfo audit)
        => new(id, accountId, name, dateOfBirth, phoneNumber, photoUrl, version, audit);
}
