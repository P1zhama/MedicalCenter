using MassTransit;
using MassTransit.MongoDbIntegration;
using MongoDB.Driver;
using Offices.Application.Common.Interfaces;
using Offices.Domain.Models;
using Offices.Infrastructure.Persistence;

namespace Offices.Infrastructure.Repositories;

public sealed class OfficeRepository : IOfficeRepository
{
    private readonly IMongoCollection<OfficeDocument> _offices;
    private readonly MongoDbContext _mongoDbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public OfficeRepository(
        OfficesDbContext context,
        MongoDbContext mongoDbContext,
        IPublishEndpoint publishEndpoint)
    {
        _offices = context.Offices;
        _mongoDbContext = mongoDbContext;
        _publishEndpoint = publishEndpoint;
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

    public async Task<bool> UpdateAsync(
        Office office,
        long expectedVersion,
        IReadOnlyCollection<object> integrationEvents,
        CancellationToken cancellationToken = default)
    {
        var session = await _mongoDbContext.StartSession(cancellationToken);
        await _mongoDbContext.BeginTransaction(cancellationToken);

        try
        {
            var result = await _offices.ReplaceOneAsync(
                session,
                document => document.Id == office.Id && document.Version == expectedVersion,
                office.ToDocument(),
                cancellationToken: cancellationToken);

            if (!result.IsAcknowledged || result.MatchedCount == 0)
            {
                await session.AbortTransactionAsync(cancellationToken);
                return false;
            }

            foreach (var integrationEvent in integrationEvents)
            {
                await _publishEndpoint.Publish(integrationEvent, integrationEvent.GetType(), cancellationToken);
            }

            await session.CommitTransactionAsync(cancellationToken);

            return true;
        }
        catch
        {
            await session.AbortTransactionAsync(cancellationToken);

            throw;
        }
    }
}
