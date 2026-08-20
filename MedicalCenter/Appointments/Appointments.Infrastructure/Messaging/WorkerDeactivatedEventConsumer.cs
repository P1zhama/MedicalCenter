using Appointments.Application.Commands.CancelUpcomingAppointments;
using MassTransit;
using MediatR;
using MedicalCenter.Shared.Contracts;
using Microsoft.Extensions.Logging;

namespace Appointments.Infrastructure.Messaging;

public sealed class WorkerDeactivatedEventConsumer : IConsumer<WorkerDeactivatedEvent>
{
    private readonly ISender _sender;
    private readonly ILogger<WorkerDeactivatedEventConsumer> _logger;

    public WorkerDeactivatedEventConsumer(ISender sender, ILogger<WorkerDeactivatedEventConsumer> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<WorkerDeactivatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Worker profile {ProfileId} deactivated — cancelling upcoming appointments.",
            message.ProfileId);

        var result = await _sender.Send(
            new CancelUpcomingByDoctorCommand(message.ProfileId),
            context.CancellationToken);

        if (result.IsError)
        {
            throw new InvalidOperationException(
                $"Failed to cancel upcoming appointments of doctor {message.ProfileId}: {result.Errors[0].Description}");
        }
    }
}
