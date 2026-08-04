using Common.Abstractions.Eventing;
using MassTransit;

namespace Common.Infrastructure.Eventing;

public sealed class EventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public EventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default)
        where TMessage : class
        => _publishEndpoint.Publish(message, message.GetType(), cancellationToken);
}
