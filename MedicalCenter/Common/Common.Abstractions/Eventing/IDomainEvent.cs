namespace Common.Abstractions.Eventing;

public interface IDomainEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredOn { get; }
}
