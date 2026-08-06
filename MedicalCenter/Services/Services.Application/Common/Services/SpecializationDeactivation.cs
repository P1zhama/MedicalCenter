using MedicalCenter.Shared.Contracts;
using Services.Application.Common.Interfaces;
using Services.Domain.Enums;
using Services.Domain.Models;

namespace Services.Application.Common.Services;

public sealed class SpecializationDeactivation
{
    private readonly IServiceCommandRepository _serviceRepository;

    public SpecializationDeactivation(IServiceCommandRepository serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<IReadOnlyCollection<object>> CascadeDeactivationAsync(
        bool deactivated,
        Specialization specialization,
        Guid updatedBy,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        if (!deactivated)
            return [];

        var integrationEvents = new List<object>
        {
            new SpecializationDeactivatedEvent(specialization.Id, now.UtcDateTime)
        };

        var services = await _serviceRepository.GetBySpecializationAsync(specialization.Id, cancellationToken);

        foreach (var service in services.Where(service => service.IsActive))
        {
            var expectedVersion = service.Version;

            service.ChangeStatus(ActivityStatus.Inactive, updatedBy, now);
            _serviceRepository.Update(service, expectedVersion);

            integrationEvents.Add(new ServiceDeactivatedEvent(service.Id, service.SpecializationId, now.UtcDateTime));
        }

        return integrationEvents;
    }
}
