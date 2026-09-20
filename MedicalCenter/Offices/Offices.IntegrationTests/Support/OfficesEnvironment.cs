using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using MongoDB.Driver;
using Offices.Api.Protos;
using Testcontainers.MongoDb;
using Testcontainers.RabbitMq;

namespace Offices.IntegrationTests.Support;

public sealed class OfficesEnvironment : IAsyncLifetime
{
    private const string ApiDatabaseName = "offices-api";
    private const string OfficesCollectionName = "Offices";
    private const string OutboxCollectionName = "outbox.messages";

    private readonly MongoDbContainer _mongo = new MongoDbBuilder("mongo:7")
        .WithReplicaSet("rs0")
        .Build();

    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder("rabbitmq:3-management")
        .WithUsername("guest")
        .WithPassword("guest")
        .Build();

    private WebApplicationFactory<Program>? _factory;
    private GrpcChannel? _channel;
    private IMongoDatabase _apiDatabase = null!;

    public string MongoConnectionString => _mongo.GetConnectionString();

    public string RabbitMqHost => _rabbitMq.Hostname;

    public ushort RabbitMqPort => _rabbitMq.GetMappedPublicPort(5672);

    public OfficesService.OfficesServiceClient Client { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        await Task.WhenAll(_mongo.StartAsync(), _rabbitMq.StartAsync());

        SetConfigurationVariable("MongoDbSettings__ConnectionString", MongoConnectionString);
        SetConfigurationVariable("MongoDbSettings__DatabaseName", ApiDatabaseName);
        SetConfigurationVariable("MongoDbSettings__OfficesCollectionName", OfficesCollectionName);
        SetConfigurationVariable("RabbitMqSettings__Host", RabbitMqHost);
        SetConfigurationVariable("RabbitMqSettings__Port", RabbitMqPort.ToString());
        SetConfigurationVariable("RabbitMqSettings__Username", "guest");
        SetConfigurationVariable("RabbitMqSettings__Password", "guest");
        SetConfigurationVariable("JwtSettings__PublicKey", TestTokens.PublicKey);
        SetConfigurationVariable("JwtSettings__Issuer", TestTokens.Issuer);
        SetConfigurationVariable("JwtSettings__Audience", TestTokens.Audience);

        _factory = new WebApplicationFactory<Program>();
        _channel = GrpcChannel.ForAddress(
            _factory.Server.BaseAddress,
            new GrpcChannelOptions { HttpHandler = _factory.Server.CreateHandler() });

        Client = new OfficesService.OfficesServiceClient(_channel);
        _apiDatabase = new MongoClient(MongoConnectionString).GetDatabase(ApiDatabaseName);
    }

    public async ValueTask DisposeAsync()
    {
        _channel?.Dispose();

        if (_factory is not null)
            await _factory.DisposeAsync();

        await _mongo.DisposeAsync();
        await _rabbitMq.DisposeAsync();
    }

    public async Task ClearApiDataAsync(CancellationToken cancellationToken)
    {
        await _apiDatabase.DropCollectionAsync(OfficesCollectionName, cancellationToken);
        await _apiDatabase.DropCollectionAsync(OutboxCollectionName, cancellationToken);
    }

    private static void SetConfigurationVariable(string key, string value)
        => Environment.SetEnvironmentVariable(key, value);
}
