using Microsoft.EntityFrameworkCore;
using Services.Application.Common.Interfaces;
using Services.Domain.Models;
using Services.Infrastructure.Persistence;
using Services.Infrastructure.Persistence.Mappers;

namespace Services.Infrastructure.Repositories;

public sealed class ServiceCommandRepository : IServiceCommandRepository
{
    private readonly ServicesDbContext _context;

    public ServiceCommandRepository(ServicesDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Service service, CancellationToken cancellationToken = default)
    {
        await _context.Services.AddAsync(service.ToEntity(), cancellationToken);
    }

    public async Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Services
            .AsNoTracking()
            .FirstOrDefaultAsync(service => service.Id == id, cancellationToken);

        return entity?.ToDomain();
    }

    public async Task<IReadOnlyList<Service>> GetBySpecializationAsync(
        Guid specializationId,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.Services
            .AsNoTracking()
            .Where(service => service.SpecializationId == specializationId)
            .ToListAsync(cancellationToken);

        return entities.ConvertAll(entity => entity.ToDomain());
    }

    public Task<bool> ExistsWithNameAsync(
        string name,
        Guid specializationId,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Services
            .AsNoTracking()
            .Where(service => service.SpecializationId == specializationId && service.Name == name);

        if (excludingId.HasValue)
            query = query.Where(service => service.Id != excludingId.Value);

        return query.AnyAsync(cancellationToken);
    }

    public void Update(Service service, long expectedVersion)
    {
        var entry = _context.Services.Attach(service.ToEntity());

        entry.State = EntityState.Modified;
        entry.Property(entity => entity.Version).OriginalValue = expectedVersion;
    }
}
