using Microsoft.EntityFrameworkCore;
using Services.Application.Common.Interfaces;
using Services.Domain.Models;
using Services.Infrastructure.Persistence;
using Services.Infrastructure.Persistence.Mappers;

namespace Services.Infrastructure.Repositories;

public sealed class SpecializationCommandRepository : ISpecializationCommandRepository
{
    private readonly ServicesDbContext _context;

    public SpecializationCommandRepository(ServicesDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Specialization specialization, CancellationToken cancellationToken = default)
    {
        await _context.Specializations.AddAsync(specialization.ToEntity(), cancellationToken);
    }

    public async Task<Specialization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Specializations
            .AsNoTracking()
            .FirstOrDefaultAsync(specialization => specialization.Id == id, cancellationToken);

        return entity?.ToDomain();
    }

    public Task<bool> ExistsWithNameAsync(
        string name,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Specializations
            .AsNoTracking()
            .Where(specialization => specialization.Name == name);

        if (excludingId.HasValue)
            query = query.Where(specialization => specialization.Id != excludingId.Value);

        return query.AnyAsync(cancellationToken);
    }

    public void Update(Specialization specialization, long expectedVersion)
    {
        var entry = _context.Specializations.Attach(specialization.ToEntity());

        entry.State = EntityState.Modified;
        entry.Property(entity => entity.Version).OriginalValue = expectedVersion;
    }
}
