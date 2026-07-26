using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Offices.Infrastructure.Persistence;

public class OfficesDbContext
{
    public IMongoCollection<OfficeDocument> Offices { get; }

    public OfficesDbContext(IOptions<MongoDbSettings> options)
    {
        var settings = options.Value;

        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);

        Offices = database.GetCollection<OfficeDocument>(settings.OfficesCollectionName);
    }
}
