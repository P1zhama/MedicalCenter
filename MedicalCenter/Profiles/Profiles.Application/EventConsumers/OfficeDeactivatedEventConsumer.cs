using MassTransit;
using MedicalCenter.Shared.Contracts;
using Microsoft.Extensions.Logging;
using Profiles.Application.Common.Interfaces;

namespace Profiles.Application.EventConsumers;

public class OfficeDeactivatedEventConsumer : IConsumer<OfficeDeactivatedEvent>
{
    private readonly IDoctorRepository _doctorRepository;
    private readonly ILogger<OfficeDeactivatedEventConsumer> _logger;

    public OfficeDeactivatedEventConsumer(
        IDoctorRepository doctorRepository,
        ILogger<OfficeDeactivatedEventConsumer> logger)
    {
        _doctorRepository = doctorRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OfficeDeactivatedEvent> context)
    {
        var officeId = context.Message.OfficeId;

        _logger.LogInformation("Office {OfficeId} deactivated — deactivating its doctors.", officeId);

        var affected = await _doctorRepository.DeactivateByOfficeAsync(officeId, context.CancellationToken);

        _logger.LogInformation("Deactivated {Count} doctor(s) of office {OfficeId}.", affected, officeId);
    }
}
