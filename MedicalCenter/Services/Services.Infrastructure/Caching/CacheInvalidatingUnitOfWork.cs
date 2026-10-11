using Services.Application.Common.Interfaces;

namespace Services.Infrastructure.Caching;

public sealed class CacheInvalidatingUnitOfWork : IUnitOfWork
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ServicesCache _cache;

    public CacheInvalidatingUnitOfWork(IUnitOfWork unitOfWork, ServicesCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<bool> TrySaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
            return false;

        await _cache.InvalidateAsync();

        return true;
    }
}
