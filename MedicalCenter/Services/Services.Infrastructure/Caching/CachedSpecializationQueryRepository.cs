using Services.Application.Common.Dtos;
using Services.Application.Common.Interfaces;

namespace Services.Infrastructure.Caching;

public sealed class CachedSpecializationQueryRepository : ISpecializationQueryRepository
{
    private readonly ISpecializationQueryRepository _repository;
    private readonly ServicesCache _cache;

    public CachedSpecializationQueryRepository(ISpecializationQueryRepository repository, ServicesCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<IReadOnlyList<SpecializationListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _cache.GetOrCreateAsync(
            ServicesCacheKeys.AllSpecializations,
            async token => await _repository.GetAllAsync(token),
            cancellationToken);

    public async Task<IReadOnlyList<PublicSpecializationDto>> GetActiveAsync(CancellationToken cancellationToken = default)
        => await _cache.GetOrCreateAsync(
            ServicesCacheKeys.ActiveSpecializations,
            async token => await _repository.GetActiveAsync(token),
            cancellationToken);

    public async Task<SpecializationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _cache.GetOrCreateAsync(
            ServicesCacheKeys.Specialization(id),
            async token => await _repository.GetByIdAsync(id, token),
            cancellationToken);

    public async Task<bool> IsActiveAsync(Guid id, CancellationToken cancellationToken = default)
        => await _cache.GetOrCreateAsync(
            ServicesCacheKeys.SpecializationIsActive(id),
            async token => await _repository.IsActiveAsync(id, token),
            cancellationToken);
}
