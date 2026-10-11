using Services.Application.Common.Dtos;
using Services.Application.Common.Interfaces;

namespace Services.Infrastructure.Caching;

public sealed class CachedServiceCategoryRepository : IServiceCategoryRepository
{
    private readonly IServiceCategoryRepository _repository;
    private readonly ServicesCache _cache;

    public CachedServiceCategoryRepository(IServiceCategoryRepository repository, ServicesCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => _repository.ExistsAsync(id, cancellationToken);

    public async Task<IReadOnlyList<ServiceCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _cache.GetOrCreateAsync(
            ServicesCacheKeys.AllServiceCategories,
            async token => await _repository.GetAllAsync(token),
            cancellationToken);
}
