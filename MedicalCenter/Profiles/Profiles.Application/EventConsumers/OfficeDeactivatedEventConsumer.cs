using MassTransit;
using MedicalCenter.Shared.Contracts;
using Microsoft.Extensions.Logging;
using Profiles.Application.Common.Interfaces;
using System.Threading.Tasks;

namespace Profiles.Application.EventConsumers;

public class OfficeDeactivatedEventConsumer : IConsumer<OfficeDeactivatedEvent>
{
    private readonly IWorkerRepository _workerRepository;
    private readonly ILogger<OfficeDeactivatedEventConsumer> _logger;

    public OfficeDeactivatedEventConsumer(
        IWorkerRepository workerRepository,
        ILogger<OfficeDeactivatedEventConsumer> logger)
    {
        _workerRepository = workerRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OfficeDeactivatedEvent> context)
    {
        var officeId = context.Message.OfficeId;

        _logger.LogInformation("Office {OfficeId} deactivated — deactivating its doctors.", officeId);

        var affected = await _workerRepository.DeactivateDoctorsByOfficeAsync(officeId, context.CancellationToken);

        _logger.LogInformation("Deactivated {Count} doctor(s) of office {OfficeId}.", affected, officeId);
    }
}
