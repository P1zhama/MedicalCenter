using Common.Infrastructure;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Offices.Application.Common.Interfaces;
using Offices.Infrastructure.Persistence;
using Offices.Infrastructure.Repositories;

namespace Offices.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCommonInfrastructure();

        var mongoSettings = configuration.GetSection(MongoDbSettings.SectionName).Get<MongoDbSettings>()
            ?? new MongoDbSettings();

        services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));

        RegisterMongoMappings();

        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoSettings.ConnectionString));
        services.AddSingleton<IMongoDatabase>(provider =>
            provider.GetRequiredService<IMongoClient>().GetDatabase(mongoSettings.DatabaseName));

        services.AddSingleton<OfficesDbContext>();
        services.AddScoped<IOfficeRepository, OfficeRepository>();

        services.AddMassTransit(x =>
        {
            x.AddMongoDbOutbox(o =>
            {
                o.QueryDelay = TimeSpan.FromSeconds(5);
                o.ClientFactory(provider => provider.GetRequiredService<IMongoClient>());
                o.DatabaseFactory(provider => provider.GetRequiredService<IMongoDatabase>());
                o.UseBusOutbox();
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitSettings = configuration.GetSection("RabbitMqSettings");

                cfg.Host(rabbitSettings["Host"] ?? "rabbitmq", rabbitSettings["VirtualHost"] ?? "/", h =>
                {
                    h.Username(rabbitSettings["Username"] ?? "guest");
                    h.Password(rabbitSettings["Password"] ?? "guest");
                });

                cfg.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter("offices", false));
            });
        });

        return services;
    }

    private static void RegisterMongoMappings()
    {
        BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

        if (!BsonClassMap.IsClassMapRegistered(typeof(OfficeDocument)))
        {
            BsonClassMap.RegisterClassMap<OfficeDocument>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }
    }
}
