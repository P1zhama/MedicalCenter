using Services.Application.Common.Dtos;

namespace Services.Application.Common.Interfaces;

public interface IServiceQueryRepository
{
    Task<ServiceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ServiceCatalogDto> GetActiveCatalogAsync(CancellationToken cancellationToken = default);

    Task<ServiceForAppointmentDto?> GetForAppointmentAsync(Guid id, CancellationToken cancellationToken = default);
}
