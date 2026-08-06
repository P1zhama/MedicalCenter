using MassTransit;
using MediatR;
using MedicalCenter.Shared.Contracts;
using Microsoft.Extensions.Logging;
using Profiles.Application.Commands.DeactivateSpecializationDoctors;

namespace Profiles.Infrastructure.Messaging;

public sealed class SpecializationDeactivatedEventConsumer : IConsumer<SpecializationDeactivatedEvent>
{
    private readonly ISender _sender;
    private readonly ILogger<SpecializationDeactivatedEventConsumer> _logger;

    public SpecializationDeactivatedEventConsumer(
        ISender sender,
        ILogger<SpecializationDeactivatedEventConsumer> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SpecializationDeactivatedEvent> context)
    {
        var specializationId = context.Message.SpecializationId;

        _logger.LogInformation(
            "Specialization {SpecializationId} deactivated — deactivating its doctors.",
            specializationId);

        var result = await _sender.Send(
            new DeactivateSpecializationDoctorsCommand(specializationId),
            context.CancellationToken);

        if (result.IsError)
            throw new InvalidOperationException(
                $"Failed to deactivate doctors of specialization {specializationId}: {result.Errors[0].Description}");
    }
}
