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
        Guard.NotNullOrWhiteSpace(name, nameof(name));
        Guard.MaxLength(name.Trim(), 200, nameof(name));
        Guard.NotEmpty(specializationId, nameof(specializationId));
        Guard.NotEmpty(categoryId, nameof(categoryId));

        return new Service(
            id,
            name.Trim(),
            price,
            specializationId,
            categoryId,
            status,
            version: 1,
            new AuditInfo(createdBy, createdAt, null, null));
    }

    public void Update(
        string name,
        Price price,
        Guid categoryId,
        ActivityStatus status,
        Guid updatedBy,
        DateTimeOffset at)
    {
        Guard.NotNullOrWhiteSpace(name, nameof(name));
        Guard.MaxLength(name.Trim(), 200, nameof(name));
        Guard.NotEmpty(categoryId, nameof(categoryId));

        Name = name.Trim();
        Price = price;
        CategoryId = categoryId;
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
