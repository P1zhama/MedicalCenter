using Appointments.Application.Commands.CancelUpcomingAppointments;
using MassTransit;
using MediatR;
using MedicalCenter.Shared.Contracts;
using Microsoft.Extensions.Logging;

namespace Appointments.Infrastructure.Messaging;

public sealed class ServiceDeactivatedEventConsumer : IConsumer<ServiceDeactivatedEvent>
{
    private readonly ISender _sender;
    private readonly ILogger<ServiceDeactivatedEventConsumer> _logger;

    public ServiceDeactivatedEventConsumer(ISender sender, ILogger<ServiceDeactivatedEventConsumer> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ServiceDeactivatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Service {ServiceId} deactivated — cancelling upcoming appointments.",
            message.ServiceId);

        var result = await _sender.Send(
            new CancelUpcomingByServiceCommand(message.ServiceId),
            context.CancellationToken);

        if (result.IsError)
        {
            throw new InvalidOperationException(
                $"Failed to cancel upcoming appointments of service {message.ServiceId}: {result.Errors[0].Description}");
        }
    }
}
