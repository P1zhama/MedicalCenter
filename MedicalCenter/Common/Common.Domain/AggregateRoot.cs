namespace Common.Domain;

public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    public long Version { get; protected set; }

    public AuditInfo Audit { get; protected set; }

    protected AggregateRoot(TId id, long version, AuditInfo audit) : base(id)
    {
        Version = version;
        Audit = audit;
    }
}
