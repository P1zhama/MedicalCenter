using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Offices.Infrastructure.Persistence;

public class OfficesDbContext
{
    public IMongoCollection<OfficeDocument> Offices { get; }

    public OfficesDbContext(IMongoDatabase database, IOptions<MongoDbSettings> options)
    {
        Offices = database.GetCollection<OfficeDocument>(options.Value.OfficesCollectionName);
    }
}
