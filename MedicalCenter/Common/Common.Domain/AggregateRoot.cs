using Common.Abstractions.Eventing;

namespace Common.Domain;

public abstract class AggregateRoot<TId> : Entity<TId> where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public long Version { get; protected set; }

    public AuditInfo Audit { get; protected set; }

    protected AggregateRoot(TId id, long version, AuditInfo audit) : base(id)
    {
        Version = version;
        Audit = audit;
    }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
