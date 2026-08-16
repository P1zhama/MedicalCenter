using Appointments.Application.Common.Dtos;

namespace Appointments.Application.Common.Interfaces;

public interface IServiceCatalogClient
{
    Task<ServiceForAppointmentDto?> GetServiceAsync(Guid serviceId, CancellationToken cancellationToken = default);
}
