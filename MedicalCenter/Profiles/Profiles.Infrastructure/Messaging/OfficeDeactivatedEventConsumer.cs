using MassTransit;
using MediatR;
using MedicalCenter.Shared.Contracts;
using Microsoft.Extensions.Logging;
using Profiles.Application.Commands.DeactivateOfficeWorkers;

namespace Profiles.Infrastructure.Messaging;

public sealed class OfficeDeactivatedEventConsumer : IConsumer<OfficeDeactivatedEvent>
{
    private readonly ISender _sender;
    private readonly ILogger<OfficeDeactivatedEventConsumer> _logger;

    public OfficeDeactivatedEventConsumer(
        ISender sender,
        ILogger<OfficeDeactivatedEventConsumer> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OfficeDeactivatedEvent> context)
    {
        var officeId = context.Message.OfficeId;

        _logger.LogInformation("Office {OfficeId} deactivated — deactivating its workers.", officeId);

        var result = await _sender.Send(
            new DeactivateOfficeWorkersCommand(officeId),
            context.CancellationToken);

        if (result.IsError)
            throw new InvalidOperationException(
                $"Failed to deactivate workers of office {officeId}: {result.Errors[0].Description}");
    }
}
