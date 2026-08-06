using Microsoft.EntityFrameworkCore;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain;
using Profiles.Infrastructure.Persistence;
using Profiles.Infrastructure.Persistence.Entities;
using Profiles.Infrastructure.Persistence.Mappers;

namespace Profiles.Infrastructure.Repositories;

public sealed class ReceptionistCommandRepository : IReceptionistCommandRepository
{
    private readonly ProfilesDbContext _context;

    public ReceptionistCommandRepository(ProfilesDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Receptionist receptionist, CancellationToken cancellationToken = default)
    {
        await _context.Receptionists.AddAsync(receptionist.ToEntity(), cancellationToken);
    }

    public async Task<Receptionist?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Receptionists
            .AsNoTracking()
            .FirstOrDefaultAsync(receptionist => receptionist.Id == id, cancellationToken);

        return entity?.ToDomain();
    }

    public async Task<IReadOnlyList<Receptionist>> GetByOfficeAsync(
        Guid officeId,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.Receptionists
            .AsNoTracking()
            .Where(receptionist => receptionist.OfficeId == officeId)
            .ToListAsync(cancellationToken);

        return entities.ConvertAll(entity => entity.ToDomain());
    }

    public void Update(Receptionist receptionist, long expectedVersion)
    {
        var entry = _context.Receptionists.Attach(receptionist.ToEntity());

        entry.State = EntityState.Modified;
        entry.Property(entity => entity.Version).OriginalValue = expectedVersion;
    }

    public void Remove(Guid id)
    {
        _context.Receptionists.Remove(new ReceptionistEntity { Id = id });
    }
}
