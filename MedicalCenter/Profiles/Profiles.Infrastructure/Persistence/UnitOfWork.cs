using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Profiles.Application.Common.Interfaces;

namespace Profiles.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ProfilesDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;

    public UnitOfWork(ProfilesDbContext context, ILogger<UnitOfWork> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> TrySaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (DbUpdateConcurrencyException exception)
        {
            _logger.LogWarning(
                exception,
                "Concurrency conflict while saving changes; {EntryCount} entry(ies) were modified by another operation.",
                exception.Entries.Count);

            return false;
        }
    }
}
