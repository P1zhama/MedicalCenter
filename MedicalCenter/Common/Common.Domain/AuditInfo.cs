namespace Common.Domain;

public sealed class AuditInfo : ValueObject
{
    public Guid CreatedBy { get; }

    public DateTime CreatedAt { get; }

    public Guid? UpdatedBy { get; }

    public DateTime? UpdatedAt { get; }

    public AuditInfo(Guid createdBy, DateTime createdAt, Guid? updatedBy, DateTime? updatedAt)
    {
        CreatedBy = createdBy;
        CreatedAt = createdAt;
        UpdatedBy = updatedBy;
        UpdatedAt = updatedAt;
    }

    public AuditInfo WithUpdate(Guid updatedBy, DateTime updatedAt)
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
