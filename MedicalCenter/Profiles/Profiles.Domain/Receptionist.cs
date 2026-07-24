using Common.Domain;
using ErrorOr;
using Profiles.Domain.ValueObjects;

namespace Profiles.Domain;

public sealed class Receptionist : AggregateRoot<Guid>
{
    private Receptionist(
        Guid id,
        Guid accountId,
        PersonName name,
        Guid officeId,
        string? photoUrl,
        long version,
        AuditInfo audit)
        : base(id, version, audit)
    {
        AccountId = accountId;
        Name = name;
        OfficeId = officeId;
        PhotoUrl = photoUrl;
    }

    public Guid AccountId { get; private set; }

    public PersonName Name { get; private set; }

    public Guid OfficeId { get; private set; }

    public string? PhotoUrl { get; private set; }

    public static ErrorOr<Receptionist> Create(
        Guid id,
        Guid accountId,
        PersonName name,
        Guid officeId,
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
            photoUrl,
            version: 1,
            new AuditInfo(createdBy, createdAt, null, null));
    }

    public static Receptionist Restore(
        Guid id,
        Guid accountId,
        PersonName name,
        Guid officeId,
        string? photoUrl,
        long version,
        AuditInfo audit)
        => new(id, accountId, name, officeId, photoUrl, version, audit);
}
