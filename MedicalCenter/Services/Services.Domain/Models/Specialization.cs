using Common.Domain;
using Services.Domain.Enums;

namespace Services.Domain.Models;

public sealed class Specialization : AggregateRoot<Guid>
{
    private Specialization(
        Guid id,
        string name,
        ActivityStatus status,
        long version,
        AuditInfo audit)
        : base(id, version, audit)
    {
        Name = name;
        Status = status;
    }

    public string Name { get; private set; }

    public ActivityStatus Status { get; private set; }

    public bool IsActive => Status == ActivityStatus.Active;

    public static Specialization Create(
        Guid id,
        string name,
        ActivityStatus status,
        Guid createdBy,
        DateTimeOffset createdAt)
    {
        Guard.NotNullOrWhiteSpace(name, nameof(name));
        Guard.MaxLength(name.Trim(), 100, nameof(name));

        return new Specialization(
            id,
            name.Trim(),
            status,
            version: 1,
            new AuditInfo(createdBy, createdAt, null, null));
    }

    public void Update(string name, ActivityStatus status, Guid updatedBy, DateTimeOffset at)
    {
        Guard.NotNullOrWhiteSpace(name, nameof(name));
        Guard.MaxLength(name.Trim(), 100, nameof(name));

        Name = name.Trim();
        Status = status;
        Audit = Audit.WithUpdate(updatedBy, at);
        Version++;
    }

    public void ChangeStatus(ActivityStatus status, Guid updatedBy, DateTimeOffset at)
    {
        Status = status;
        Audit = Audit.WithUpdate(updatedBy, at);
        Version++;
    }

    public static Specialization Restore(
        Guid id,
        string name,
        ActivityStatus status,
        long version,
        AuditInfo audit)
        => new(id, name, status, version, audit);
}
