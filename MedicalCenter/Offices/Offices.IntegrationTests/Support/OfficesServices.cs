using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using Offices.Infrastructure;

namespace Offices.IntegrationTests.Support;

public sealed class OfficesServices : IAsyncDisposable
{
    private readonly ServiceProvider _provider;

    private OfficesServices(ServiceProvider provider)
    {
        _provider = provider;
    }

    public static OfficesServices Create(string connectionString, string databaseName)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MongoDbSettings:ConnectionString"] = connectionString,
                ["MongoDbSettings:DatabaseName"] = databaseName,
                ["MongoDbSettings:OfficesCollectionName"] = "Offices",
                ["RabbitMqSettings:Host"] = "localhost"
            })
            .Build();

        var services = new ServiceCollection();

        services.AddLogging();
        services.AddInfrastructure(configuration);

        return new OfficesServices(services.BuildServiceProvider());
    }

    public IMongoDatabase Database => _provider.GetRequiredService<IMongoDatabase>();

    public IMongoCollection<BsonDocument> OutboxMessages => Database.GetCollection<BsonDocument>("outbox.messages");

    public AsyncServiceScope CreateScope() => _provider.CreateAsyncScope();

    public ValueTask DisposeAsync() => _provider.DisposeAsync();
}
