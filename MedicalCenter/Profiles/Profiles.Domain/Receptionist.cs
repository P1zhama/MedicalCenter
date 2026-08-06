using Common.Domain;
using ErrorOr;
using Profiles.Domain.Enums;
using Profiles.Domain.ValueObjects;

namespace Profiles.Domain;

public sealed class Receptionist : AggregateRoot<Guid>
{
    private Receptionist(
        Guid id,
        Guid accountId,
        PersonName name,
        Guid officeId,
        ReceptionistStatus status,
        string? photoUrl,
        long version,
        AuditInfo audit)
        : base(id, version, audit)
    {
        AccountId = accountId;
        Name = name;
        OfficeId = officeId;
        Status = status;
        PhotoUrl = photoUrl;
    }

    public Guid AccountId { get; private set; }

    public PersonName Name { get; private set; }

    public Guid OfficeId { get; private set; }

    public ReceptionistStatus Status { get; private set; }

    public string? PhotoUrl { get; private set; }

    public bool IsActive => Status == ReceptionistStatus.Active;

    public static ErrorOr<Receptionist> Create(
        Guid id,
        Guid accountId,
        PersonName name,
        Guid officeId,
        ReceptionistStatus status,
        string? photoUrl,
        Guid createdBy,
        DateTimeOffset createdAt)
    {
        if (accountId == Guid.Empty)
            return Error.Validation("Receptionist.AccountId", "Account id must not be empty.");

        if (officeId == Guid.Empty)
            return Error.Validation("Receptionist.OfficeId", "Please, choose the office");

        return new Receptionist(
            id,
            accountId,
            name,
            officeId,
            status,
            photoUrl,
            version: 1,
            new AuditInfo(createdBy, createdAt, null, null));
    }

    public ErrorOr<StatusTransition> Update(
        PersonName name,
        Guid officeId,
        ReceptionistStatus status,
        string? photoUrl,
        Guid updatedBy,
        DateTimeOffset at)
    {
        if (officeId == Guid.Empty)
            return Error.Validation("Receptionist.OfficeId", "Please, choose the office");

        var transition = DetectTransition(status);

        Name = name;
        OfficeId = officeId;
        Status = status;
        PhotoUrl = photoUrl;
        Audit = Audit.WithUpdate(updatedBy, at);
        Version++;

        return transition;
    }

    public StatusTransition ChangeStatus(ReceptionistStatus status, Guid updatedBy, DateTimeOffset at)
    {
        var transition = DetectTransition(status);

        Status = status;
        Audit = Audit.WithUpdate(updatedBy, at);
        Version++;

        return transition;
    }

    private StatusTransition DetectTransition(ReceptionistStatus status)
    {
        if (IsActive && status == ReceptionistStatus.Inactive)
            return StatusTransition.Deactivated;

        if (!IsActive && status == ReceptionistStatus.Active)
            return StatusTransition.Reactivated;

        return StatusTransition.None;
    }

    public static Receptionist Restore(
        Guid id,
        Guid accountId,
        PersonName name,
        Guid officeId,
        ReceptionistStatus status,
        string? photoUrl,
        long version,
        AuditInfo audit)
        => new(id, accountId, name, officeId, status, photoUrl, version, audit);
}
