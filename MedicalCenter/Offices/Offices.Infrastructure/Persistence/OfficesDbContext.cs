using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Offices.Domain;

namespace Offices.Infrastructure.Persistence;

public class OfficesDbContext
{
    public IMongoCollection<Office> Offices { get; }

    public OfficesDbContext(IOptions<MongoDbSettings> options)
    {
        var settings = options.Value;

        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);

        Offices = database.GetCollection<Office>(settings.OfficesCollectionName);
    }
}
