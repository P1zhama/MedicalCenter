using Common.Infrastructure;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Offices.Application.Common.Interfaces;
using Offices.Infrastructure.Messaging;
using Offices.Infrastructure.Persistence;
using Offices.Infrastructure.Repositories;
using Offices.Infrastructure.Security;

namespace Offices.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCommonInfrastructure();

        services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));

        RegisterMongoMappings();

        services.AddSingleton<OfficesDbContext>();
        services.AddScoped<IOfficeRepository, OfficeRepository>();
        services.AddScoped<CurrentUserProvider>();
        services.AddScoped<ICurrentUserProvider>(provider => provider.GetRequiredService<CurrentUserProvider>());
        services.AddScoped<IEventPublisher, EventPublisher>();

        services.AddMassTransit(x =>
        {
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
