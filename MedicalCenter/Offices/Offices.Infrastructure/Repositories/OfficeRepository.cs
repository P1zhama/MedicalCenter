using MongoDB.Driver;
using Offices.Application.Common.Interfaces;
using Offices.Domain;
using Offices.Infrastructure.Persistence;

namespace Offices.Infrastructure.Repositories;

public sealed class OfficeRepository : IOfficeRepository
{
    private readonly IMongoCollection<OfficeDocument> _offices;

    public OfficeRepository(OfficesDbContext context)
    {
        _offices = context.Offices;
    }

    public Task AddAsync(Office office, CancellationToken cancellationToken = default)
        => _offices.InsertOneAsync(office.ToDocument(), cancellationToken: cancellationToken);

    public async Task<Office?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _offices.Find(office => office.Id == id).FirstOrDefaultAsync(cancellationToken);

        return document?.ToDomain();
    }

    public async Task<IReadOnlyList<Office>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _offices.Find(FilterDefinition<OfficeDocument>.Empty).ToListAsync(cancellationToken);

        return documents.Select(document => document.ToDomain()).ToList();
    }

    public async Task<bool> UpdateAsync(Office office, long expectedVersion, CancellationToken cancellationToken = default)
    {
        var result = await _offices.ReplaceOneAsync(
            document => document.Id == office.Id && document.Version == expectedVersion,
            office.ToDocument(),
            cancellationToken: cancellationToken);

        return result.IsAcknowledged && result.ModifiedCount == 1;
    }
}
