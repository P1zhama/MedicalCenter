using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Offices.Application.Common.Interfaces;
using Offices.Domain;
using Offices.Domain.Enums;
using Offices.Infrastructure.Persistence;
using Offices.Infrastructure.Repositories;

namespace Offices.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));

        RegisterMongoMappings();

        services.AddSingleton<OfficesDbContext>();
        services.AddScoped<IOfficeRepository, OfficeRepository>();

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

        if (!BsonClassMap.IsClassMapRegistered(typeof(Office)))
        {
            BsonClassMap.RegisterClassMap<Office>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
                cm.MapMember(o => o.Status).SetSerializer(new EnumSerializer<OfficeStatus>(BsonType.String));
            });
        }
    }
}
