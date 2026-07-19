using Common.Abstractions.Eventing;

namespace Authorization.Domain.Events;

public sealed record AccountRegisteredDomainEvent(Guid AccountId, string Email, DateTime OccurredOnUtc)
    : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
}
