using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Services.Infrastructure.Caching;

public sealed class ServicesCache
{
    public const string Tag = "services";

    private static readonly string[] Tags = [Tag];

    private readonly HybridCache _cache;
    private readonly ILogger<ServicesCache> _logger;

    public ServicesCache(HybridCache cache, ILogger<ServicesCache> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async ValueTask<T> GetOrCreateAsync<T>(
        string key,
        Func<CancellationToken, ValueTask<T>> readFromDatabase,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _cache.GetOrCreateAsync(
                key,
                readFromDatabase,
                static async (factory, token) =>
                {
                    try
                    {
                        return await factory(token);
                    }
                    catch (Exception exception)
                    {
                        throw new DatabaseReadFailedException(exception);
                    }
                },
                tags: Tags,
                cancellationToken: cancellationToken);
        }
        catch (DatabaseReadFailedException exception)
        {
            ExceptionDispatchInfo.Throw(exception.InnerException!);
            throw;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogWarning(
                exception,
                "Cache is unavailable for key {CacheKey}; reading from the database instead.",
                key);

            return await readFromDatabase(cancellationToken);
        }
    }

    public async ValueTask InvalidateAsync()
    {
        try
        {
            await _cache.RemoveByTagAsync(Tag, CancellationToken.None);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Cache invalidation failed; cached entries will be served until they expire.");
        }
    }

    private sealed class DatabaseReadFailedException : Exception
    {
        public DatabaseReadFailedException(Exception inner)
            : base(inner.Message, inner)
        {
        }
    }
}
