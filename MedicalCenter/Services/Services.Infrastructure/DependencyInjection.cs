using Common.Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Services.Application.Common.Interfaces;
using Services.Infrastructure.Caching;
using Services.Infrastructure.Persistence;
using Services.Infrastructure.Repositories;
using StackExchange.Redis;

namespace Services.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCommonInfrastructure();

        services.AddDbContext<ServicesDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddCache(configuration);

        services.AddScoped<ISpecializationCommandRepository, SpecializationCommandRepository>();
        services.AddScoped<IServiceCommandRepository, ServiceCommandRepository>();

        services.AddScoped<SpecializationQueryRepository>();
        services.AddScoped<ISpecializationQueryRepository>(provider => new CachedSpecializationQueryRepository(
            provider.GetRequiredService<SpecializationQueryRepository>(),
            provider.GetRequiredService<ServicesCache>()));

        services.AddScoped<ServiceQueryRepository>();
        services.AddScoped<IServiceQueryRepository>(provider => new CachedServiceQueryRepository(
            provider.GetRequiredService<ServiceQueryRepository>(),
            provider.GetRequiredService<ServicesCache>()));

        services.AddScoped<ServiceCategoryRepository>();
        services.AddScoped<IServiceCategoryRepository>(provider => new CachedServiceCategoryRepository(
            provider.GetRequiredService<ServiceCategoryRepository>(),
            provider.GetRequiredService<ServicesCache>()));

        services.AddScoped<UnitOfWork>();
        services.AddScoped<IUnitOfWork>(provider => new CacheInvalidatingUnitOfWork(
            provider.GetRequiredService<UnitOfWork>(),
            provider.GetRequiredService<ServicesCache>()));

        services.AddMassTransit(bus =>
        {
            bus.AddEntityFrameworkOutbox<ServicesDbContext>(outbox =>
            {
                outbox.UseSqlServer();
                outbox.UseBusOutbox();
            });

            bus.UsingRabbitMq((context, cfg) =>
            {
                var rabbit = configuration.GetSection("RabbitMqSettings");

                cfg.Host(rabbit["Host"] ?? "rabbitmq", rabbit["VirtualHost"] ?? "/", host =>
                {
                    host.Username(rabbit["Username"] ?? "guest");
                    host.Password(rabbit["Password"] ?? "guest");
                });

                cfg.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter("services", false));
            });
        });

        return services;
    }

    private static IServiceCollection AddCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redis = configuration.GetSection("Redis");
        var connectionString = redis["ConnectionString"];

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                var redisOptions = ConfigurationOptions.Parse(connectionString);

                redisOptions.AbortOnConnectFail = false;

                redisOptions.ConnectRetry = 1;
                redisOptions.ConnectTimeout = 1000;
                redisOptions.SyncTimeout = 1000;
                redisOptions.AsyncTimeout = 1000;

                options.ConfigurationOptions = redisOptions;
                options.InstanceName = redis["InstanceName"] ?? "services:";
            });
        }

        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(5),
                LocalCacheExpiration = TimeSpan.FromMinutes(1)
            };
        });

        services.AddSingleton<ServicesCache>();

        return services;
    }
}
