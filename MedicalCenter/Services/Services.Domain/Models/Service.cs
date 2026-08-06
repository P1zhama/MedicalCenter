using Common.Domain;
using Services.Domain.Enums;
using Services.Domain.ValueObjects;

namespace Services.Domain.Models;

public sealed class Service : AggregateRoot<Guid>
{
    private Service(
        Guid id,
        string name,
        Price price,
        Guid specializationId,
        Guid categoryId,
        ActivityStatus status,
        long version,
        AuditInfo audit)
        : base(id, version, audit)
    {
        Name = name;
        Price = price;
        SpecializationId = specializationId;
        CategoryId = categoryId;
        Status = status;
    }

    public string Name { get; private set; }

    public Price Price { get; private set; }

    public Guid SpecializationId { get; private set; }

    public Guid CategoryId { get; private set; }

    public ActivityStatus Status { get; private set; }

    public bool IsActive => Status == ActivityStatus.Active;

    public static Service Create(
        Guid id,
        string name,
        Price price,
        Guid specializationId,
        Guid categoryId,
        ActivityStatus status,
        Guid createdBy,
        DateTimeOffset createdAt)
    {
        var normalizedName = TextNormalization.CollapseWhitespace(Guard.NotNullOrWhiteSpace(name, nameof(name)));
        Guard.MaxLength(normalizedName, 200, nameof(name));
        Guard.NotEmpty(specializationId, nameof(specializationId));
        Guard.NotEmpty(categoryId, nameof(categoryId));

        return new Service(
            id,
            normalizedName,
            price,
            specializationId,
            categoryId,
            status,
            version: 1,
            new AuditInfo(createdBy, createdAt, null, null));
    }

    public bool Update(
        string name,
        Price price,
        Guid categoryId,
        ActivityStatus status,
        Guid updatedBy,
        DateTimeOffset at)
    {
        var normalizedName = TextNormalization.CollapseWhitespace(Guard.NotNullOrWhiteSpace(name, nameof(name)));
        Guard.MaxLength(normalizedName, 200, nameof(name));
        Guard.NotEmpty(categoryId, nameof(categoryId));

        var deactivated = IsDeactivating(status);

        Name = normalizedName;
        Price = price;
        CategoryId = categoryId;
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

    public static Service Restore(
        Guid id,
        string name,
        Price price,
        Guid specializationId,
        Guid categoryId,
        ActivityStatus status,
        long version,
        AuditInfo audit)
        => new(id, name, price, specializationId, categoryId, status, version, audit);
}
