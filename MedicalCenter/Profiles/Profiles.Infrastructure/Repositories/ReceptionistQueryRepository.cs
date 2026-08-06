using Microsoft.EntityFrameworkCore;
using Profiles.Application.Common.Dtos;
using Profiles.Application.Common.Interfaces;
using Profiles.Infrastructure.Persistence;
using Profiles.Infrastructure.Persistence.Entities;

namespace Profiles.Infrastructure.Repositories;

public sealed class ReceptionistQueryRepository : IReceptionistQueryRepository
{
    private readonly ProfilesDbContext _context;

    public ReceptionistQueryRepository(ProfilesDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ReceptionistListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Receptionists
            .AsNoTracking()
            .OrderBy(receptionist => receptionist.LastName)
            .ThenBy(receptionist => receptionist.FirstName)
            .Select(receptionist => new ReceptionistListItemDto(
                receptionist.Id,
                receptionist.FirstName,
                receptionist.LastName,
                receptionist.MiddleName,
                receptionist.OfficeId,
                receptionist.Status))
            .ToListAsync(cancellationToken);

    public Task<ReceptionistDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Project(_context.Receptionists.AsNoTracking().Where(receptionist => receptionist.Id == id))
            .FirstOrDefaultAsync(cancellationToken)!;

    public Task<ReceptionistDto?> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default)
        => Project(_context.Receptionists.AsNoTracking().Where(receptionist => receptionist.AccountId == accountId))
            .FirstOrDefaultAsync(cancellationToken)!;

    private static IQueryable<ReceptionistDto> Project(IQueryable<ReceptionistEntity> query)
        => query.Select(receptionist => new ReceptionistDto(
            receptionist.Id,
            receptionist.PhotoUrl,
            receptionist.FirstName,
            receptionist.LastName,
            receptionist.MiddleName,
            receptionist.OfficeId,
            receptionist.Status));
}
