using Appointments.Application.Common.Dtos;

namespace Appointments.Application.Common.Interfaces;

public interface IServiceCatalogClient
{
    Task<ServiceForAppointmentDto?> GetServiceAsync(Guid serviceId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ServiceSummaryDto>> GetSummariesAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);
}
