namespace Common.Domain;

public abstract class Entity<TId> where TId : notnull
{
    public TId Id { get; }

    protected Entity(TId id)
    {
        Id = id;
    }

    public override bool Equals(object? obj)
    {
        return obj is Entity<TId> other
            && GetType() == other.GetType()
            && Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Id);
    }
}
