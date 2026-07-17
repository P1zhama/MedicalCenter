using Authorization.API.Protos;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Profiles.Application.Common.Interfaces;
using Profiles.Application.EventConsumers;
using Profiles.Infrastructure.Persistence;
using Profiles.Infrastructure.Repositories;
using Profiles.Infrastructure.Services;
using System;

namespace Profiles.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IWorkerRepository, WorkerRepository>();

        services.AddGrpcClient<AuthInternalService.AuthInternalServiceClient>(options =>
        {
            options.Address = new Uri(configuration["GrpcClients:Authorization"] ?? "http://localhost:8000");
        });
        services.AddScoped<IAuthorizationServiceClient, AuthorizationServiceClient>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<OfficeDeactivatedEventConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitSettings = configuration.GetSection("RabbitMqSettings");

                cfg.Host(rabbitSettings["Host"] ?? "rabbitmq", rabbitSettings["VirtualHost"] ?? "/", h =>
                {
                    h.Username(rabbitSettings["Username"] ?? "guest");
                    h.Password(rabbitSettings["Password"] ?? "guest");
                });

                cfg.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter("profiles", false));
            });
        });

        return services;
    }
}