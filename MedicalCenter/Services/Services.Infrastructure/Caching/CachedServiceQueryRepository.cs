using Services.Application.Common.Dtos;
using Services.Application.Common.Interfaces;

namespace Services.Infrastructure.Caching;

public sealed class CachedServiceQueryRepository : IServiceQueryRepository
{
    private readonly IServiceQueryRepository _repository;
    private readonly ServicesCache _cache;

    public CachedServiceQueryRepository(IServiceQueryRepository repository, ServicesCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<ServiceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _cache.GetOrCreateAsync(
            ServicesCacheKeys.Service(id),
            async token => await _repository.GetByIdAsync(id, token),
            cancellationToken);

    public async Task<ServiceCatalogDto> GetActiveCatalogAsync(CancellationToken cancellationToken = default)
        => await _cache.GetOrCreateAsync(
            ServicesCacheKeys.ServiceCatalog,
            async token => await _repository.GetActiveCatalogAsync(token),
            cancellationToken);

    public Task<ServiceForAppointmentDto?> GetForAppointmentAsync(Guid id, CancellationToken cancellationToken = default)
        => _repository.GetForAppointmentAsync(id, cancellationToken);

    public Task<IReadOnlyList<ServiceSummaryDto>> GetSummariesAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
        => _repository.GetSummariesAsync(ids, cancellationToken);
}
