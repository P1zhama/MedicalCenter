using MongoDB.Driver;
using Offices.Application.Common.Interfaces;
using Offices.Domain;
using Offices.Infrastructure.Persistence;

namespace Offices.Infrastructure.Repositories;

public class OfficeRepository : IOfficeRepository
{
    private readonly IMongoCollection<Office> _offices;

    public OfficeRepository(OfficesDbContext context)
    {
        _offices = context.Offices;
    }

    public async Task AddAsync(Office office, CancellationToken cancellationToken)
    {
        await _offices.InsertOneAsync(office, cancellationToken: cancellationToken);
    }

    public async Task<Office?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _offices.Find(o => o.Id == id).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Office>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _offices.Find(FilterDefinition<Office>.Empty).ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(Office office, CancellationToken cancellationToken)
    {
        await _offices.ReplaceOneAsync(o => o.Id == office.Id, office, cancellationToken: cancellationToken);
    }
}
