using Authorization.Api.Protos;
using Common.Infrastructure;
using MassTransit;
using Offices.Api.Protos;
using Services.Api.Protos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Profiles.Application.Common.Interfaces;
using Profiles.Infrastructure.Messaging;
using Profiles.Infrastructure.Persistence;
using Profiles.Infrastructure.Repositories;
using Profiles.Infrastructure.Services;

namespace Profiles.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCommonInfrastructure();

        services.AddDbContext<ProfilesDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IPatientCommandRepository, PatientCommandRepository>();
        services.AddScoped<IPatientQueryRepository, PatientQueryRepository>();
        services.AddScoped<IDoctorCommandRepository, DoctorCommandRepository>();
        services.AddScoped<IDoctorQueryRepository, DoctorQueryRepository>();
        services.AddScoped<IReceptionistCommandRepository, ReceptionistCommandRepository>();
        services.AddScoped<IReceptionistQueryRepository, ReceptionistQueryRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddGrpcClient<AuthInternalService.AuthInternalServiceClient>(options =>
        {
            options.Address = new Uri(configuration["GrpcClients:Authorization"] ?? "http://localhost:8000");
        });
        services.AddScoped<IAuthorizationServiceClient, AuthorizationServiceClient>();

        services.AddGrpcClient<OfficesService.OfficesServiceClient>(options =>
        {
            options.Address = new Uri(configuration["GrpcClients:Offices"] ?? "http://localhost:8004");
        });
        services.AddScoped<IOfficeServiceClient, OfficeServiceClient>();

        services.AddGrpcClient<ServicesService.ServicesServiceClient>(options =>
        {
            options.Address = new Uri(configuration["GrpcClients:Services"] ?? "http://localhost:8002");
        });
        services.AddScoped<ISpecializationServiceClient, SpecializationServiceClient>();

        services.AddMassTransit(x =>
        {
            x.AddConsumer<OfficeDeactivatedEventConsumer>();
            x.AddConsumer<SpecializationDeactivatedEventConsumer>();

            x.AddEntityFrameworkOutbox<ProfilesDbContext>(outbox =>
            {
                outbox.UseSqlServer();
                outbox.UseBusOutbox();
            });

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