using Common.Abstractions.Eventing;

namespace Authorization.Domain.Events;

public sealed record SignUpDomainEvent(Guid AccountId, string Email, DateTimeOffset OccurredOn)
    : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
}
