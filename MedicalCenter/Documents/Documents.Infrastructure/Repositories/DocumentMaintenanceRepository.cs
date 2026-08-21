using Documents.Application.Common.Dtos;
using Documents.Application.Common.Interfaces;
using Documents.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Documents.Infrastructure.Repositories;

public sealed class DocumentMaintenanceRepository : IDocumentMaintenanceRepository
{
    private readonly DocumentsDbContext _context;

    public DocumentMaintenanceRepository(DocumentsDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SweepCandidateDto>> GetSweepCandidatesAsync(
        DateTimeOffset createdBefore,
        DateTimeOffset verifiedBefore,
        int limit,
        CancellationToken cancellationToken = default)
    {
        return await _context.Documents
            .AsNoTracking()
            .Where(document => document.CreatedAt < createdBefore)
            .Where(document => document.LastVerifiedAt == null || document.LastVerifiedAt < verifiedBefore)
            .OrderBy(document => document.LastVerifiedAt)
            .ThenBy(document => document.CreatedAt)
            .Take(limit)
            .Select(document => new SweepCandidateDto(document.Id, document.ObjectKey))
            .ToListAsync(cancellationToken);
    }

    public async Task MarkVerifiedAsync(
        IReadOnlyCollection<Guid> ids,
        DateTimeOffset verifiedAt,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return;

        await _context.Documents
            .Where(document => ids.Contains(document.Id))
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(document => document.LastVerifiedAt, verifiedAt),
                cancellationToken);
    }

    public async Task DeleteAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return;

        await _context.Documents
            .Where(document => ids.Contains(document.Id))
            .ExecuteDeleteAsync(cancellationToken);
    }
}
