using Common.Domain;
using Offices.Domain.Enums;
using Offices.Domain.ValueObjects;

namespace Offices.Domain.Models;

public sealed class Office : AggregateRoot<Guid>
{
    private Office(
        Guid id,
        Address address,
        string registryPhoneNumber,
        string? photoUrl,
        OfficeStatus status,
        long version,
        AuditInfo audit)
        : base(id, version, audit)
    {
        Address = address;
        RegistryPhoneNumber = registryPhoneNumber;
        PhotoUrl = photoUrl;
        Status = status;
    }

    public Address Address { get; private set; }

    public string RegistryPhoneNumber { get; private set; }

    public string? PhotoUrl { get; private set; }

    public OfficeStatus Status { get; private set; }

    public bool IsActive => Status == OfficeStatus.Active;

    public static Office Create(
        Guid id,
        Address address,
        string registryPhoneNumber,
        string? photoUrl,
        OfficeStatus status,
        Guid createdBy,
        DateTimeOffset createdAt)
    {
        Guard.NotNullOrWhiteSpace(registryPhoneNumber, nameof(registryPhoneNumber));

        return new Office(
            id,
            address,
            registryPhoneNumber.Trim(),
            photoUrl,
            status,
            version: 1,
            new AuditInfo(createdBy, createdAt, null, null));
    }

    public void Update(
        Address address,
        string registryPhoneNumber,
        string? photoUrl,
        OfficeStatus status,
        Guid updatedBy,
        DateTimeOffset at)
    {
        Guard.NotNullOrWhiteSpace(registryPhoneNumber, nameof(registryPhoneNumber));

        Address = address;
        RegistryPhoneNumber = registryPhoneNumber.Trim();
        PhotoUrl = photoUrl;
        Status = status;
        Audit = Audit.WithUpdate(updatedBy, at);
        Version++;
    }

    public void ChangeStatus(OfficeStatus status, Guid updatedBy, DateTimeOffset at)
    {
        Status = status;
        Audit = Audit.WithUpdate(updatedBy, at);
        Version++;
    }

    public static Office Restore(
        Guid id,
        Address address,
        string registryPhoneNumber,
        string? photoUrl,
        OfficeStatus status,
        long version,
        AuditInfo audit)
        => new(id, address, registryPhoneNumber, photoUrl, status, version, audit);
}
