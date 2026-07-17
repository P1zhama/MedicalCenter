using Authorization.Application.Common.Messages;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Authorization.Application.Events;

public class WorkerCreatedEventHandler : INotificationHandler<WorkerCreatedEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<WorkerCreatedEventHandler> _logger;

    public WorkerCreatedEventHandler(IPublishEndpoint publishEndpoint, ILogger<WorkerCreatedEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Handle(WorkerCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling WorkerCreatedEvent for {Email}", notification.Email);

        string subject = "Welcome to the Clinic System!";
        string body = $@"
            <h3>Hello!</h3>
            <p>Your {notification.RoleName} account has been successfully created.</p>
            <p><strong>Login:</strong> {notification.Email}</p>
            <p><strong>Password:</strong> {notification.GeneratedPassword}</p>
            <p>Please change your password after your first login.</p>";


        await _publishEndpoint.Publish(new EmailDeliveryRequested(notification.Email, subject, body), cancellationToken);
    }
}
