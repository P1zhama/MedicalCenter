namespace Common.Domain;

public sealed class AuditInfo : ValueObject
{
    public Guid CreatedBy { get; }

    public DateTimeOffset CreatedAt { get; }

    public Guid? UpdatedBy { get; }

    public DateTimeOffset? UpdatedAt { get; }

    public AuditInfo(Guid createdBy, DateTimeOffset createdAt, Guid? updatedBy, DateTimeOffset? updatedAt)
    {
        CreatedBy = createdBy;
        CreatedAt = createdAt;
        UpdatedBy = updatedBy;
        UpdatedAt = updatedAt;
    }

    public AuditInfo WithUpdate(Guid updatedBy, DateTimeOffset updatedAt)
    {
        return new AuditInfo(CreatedBy, CreatedAt, updatedBy, updatedAt);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CreatedBy;
        yield return CreatedAt;
        yield return UpdatedBy;
        yield return UpdatedAt;
    }
}
