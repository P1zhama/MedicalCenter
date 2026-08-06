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
        var normalizedName = TextNormalization.CollapseWhitespace(Guard.NotNullOrWhiteSpace(name, nameof(name)));
        Guard.MaxLength(normalizedName, 100, nameof(name));

        return new Specialization(
            id,
            normalizedName,
            status,
            version: 1,
            new AuditInfo(createdBy, createdAt, null, null));
    }

    public bool Update(string name, ActivityStatus status, Guid updatedBy, DateTimeOffset at)
    {
        var normalizedName = TextNormalization.CollapseWhitespace(Guard.NotNullOrWhiteSpace(name, nameof(name)));
        Guard.MaxLength(normalizedName, 100, nameof(name));

        var deactivated = IsDeactivating(status);

        Name = normalizedName;
        Status = status;
        Audit = Audit.WithUpdate(updatedBy, at);
        Version++;

        return deactivated;
    }

    public bool ChangeStatus(ActivityStatus status, Guid updatedBy, DateTimeOffset at)
    {
        var deactivated = IsDeactivating(status);

        Status = status;
        Audit = Audit.WithUpdate(updatedBy, at);
        Version++;

        return deactivated;
    }

    private bool IsDeactivating(ActivityStatus status)
        => IsActive && status != ActivityStatus.Active;

    public static Specialization Restore(
        Guid id,
        string name,
        ActivityStatus status,
        long version,
        AuditInfo audit)
        => new(id, name, status, version, audit);
}
