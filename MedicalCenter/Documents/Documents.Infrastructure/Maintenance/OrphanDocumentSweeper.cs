using Documents.Application.Common.Dtos;
using Documents.Application.Common.Interfaces;
using Documents.Application.Common.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Documents.Infrastructure.Maintenance;

public sealed class OrphanDocumentSweeper : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DocumentCleanupSettings _settings;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<OrphanDocumentSweeper> _logger;

    public OrphanDocumentSweeper(
        IServiceScopeFactory scopeFactory,
        IOptions<DocumentCleanupSettings> settings,
        TimeProvider timeProvider,
        ILogger<OrphanDocumentSweeper> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Orphan document sweeper is disabled.");

            return;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_settings.IntervalMinutes));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await SweepAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Orphan document sweep failed; nothing was deleted this cycle.");
            }
        }
    }

    private async Task SweepAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IDocumentMaintenanceRepository>();
        var references = scope.ServiceProvider.GetRequiredService<IPhotoReferenceClient>();
        var storage = scope.ServiceProvider.GetRequiredService<IFileStorage>();

        var now = _timeProvider.GetUtcNow();

        var candidates = await repository.GetSweepCandidatesAsync(
            now.AddHours(-_settings.MinimumAgeHours),
            now.AddDays(-_settings.RevalidateAfterDays),
            _settings.BatchSize,
            cancellationToken);

        if (candidates.Count == 0)
            return;

        var ids = candidates.Select(candidate => candidate.Id).ToList();
        var referenced = (await references.GetReferencedAsync(ids, cancellationToken)).ToHashSet();

        var keep = ids.Where(referenced.Contains).ToList();
        var orphans = candidates.Where(candidate => !referenced.Contains(candidate.Id)).ToList();

        await repository.MarkVerifiedAsync(keep, now, cancellationToken);

        var deleted = await DeleteObjectsAsync(storage, orphans, cancellationToken);

        await repository.DeleteAsync(deleted, cancellationToken);

        _logger.LogInformation(
            "Swept {CandidateCount} document(s): {KeptCount} still referenced, {DeletedCount} deleted.",
            candidates.Count,
            keep.Count,
            deleted.Count);
    }

    private async Task<IReadOnlyCollection<Guid>> DeleteObjectsAsync(
        IFileStorage storage,
        IReadOnlyCollection<SweepCandidateDto> orphans,
        CancellationToken cancellationToken)
    {
        var deleted = new List<Guid>(orphans.Count);

        foreach (var orphan in orphans)
        {
            try
            {
                await storage.DeleteAsync(orphan.ObjectKey, cancellationToken);

                deleted.Add(orphan.Id);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Could not remove object {ObjectKey}; its record is kept for the next sweep.",
                    orphan.ObjectKey);
            }
        }

        return deleted;
    }
}
