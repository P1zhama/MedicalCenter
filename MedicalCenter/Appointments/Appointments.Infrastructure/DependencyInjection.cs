using Appointments.Application.Common.Interfaces;
using Appointments.Application.Common.Services;
using Appointments.Application.Common.Settings;
using Appointments.Domain.Scheduling;
using Appointments.Infrastructure.Clients;
using Common.Infrastructure.Interceptors;
using Appointments.Infrastructure.Messaging;
using Appointments.Infrastructure.Persistence;
using Appointments.Infrastructure.Repositories;
using Common.Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Profiles.Api.Protos;
using Services.Api.Protos;

namespace Appointments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCommonInfrastructure();

        services.AddDbContext<AppointmentsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        var workingHours = configuration.GetSection(WorkingHoursSettings.SectionName).Get<WorkingHoursSettings>()
            ?? throw new InvalidOperationException($"Section '{WorkingHoursSettings.SectionName}' is missing.");

        var clinicTimeZone = ResolveTimeZone(workingHours.TimeZone);

        services.Configure<WorkingHoursSettings>(configuration.GetSection(WorkingHoursSettings.SectionName));
        services.AddSingleton(CreateWorkingSchedule(workingHours));
        services.AddSingleton(provider => new ClinicClock(provider.GetRequiredService<TimeProvider>(), clinicTimeZone));

        services.AddScoped<IAppointmentCommandRepository, AppointmentCommandRepository>();
        services.AddScoped<IAppointmentQueryRepository, AppointmentQueryRepository>();
        services.AddScoped<IAppointmentResultCommandRepository, AppointmentResultCommandRepository>();
        services.AddScoped<IAppointmentResultQueryRepository, AppointmentResultQueryRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHttpContextAccessor();
        services.AddTransient<TokenForwardingInterceptor>();

        services.AddGrpcClient<ProfilesService.ProfilesServiceClient>(options =>
        {
            options.Address = new Uri(configuration["GrpcClients:Profiles"] ?? "http://localhost:8001");
        }).AddInterceptor<TokenForwardingInterceptor>();

        services.AddGrpcClient<ServicesService.ServicesServiceClient>(options =>
        {
            options.Address = new Uri(configuration["GrpcClients:Services"] ?? "http://localhost:8002");
        }).AddInterceptor<TokenForwardingInterceptor>();

        services.AddScoped<IDoctorDirectoryClient, DoctorDirectoryClient>();
        services.AddScoped<IServiceCatalogClient, ServiceCatalogClient>();
        services.AddScoped<IPatientDirectoryClient, PatientDirectoryClient>();

        services.AddMassTransit(bus =>
        {
            bus.AddConsumer<WorkerDeactivatedEventConsumer>();
            bus.AddConsumer<ServiceDeactivatedEventConsumer>();

            bus.AddEntityFrameworkOutbox<AppointmentsDbContext>(outbox =>
            {
                outbox.UsePostgres();
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

                cfg.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter("appointments", false));
            });
        });

        return services;
    }

    private static TimeZoneInfo ResolveTimeZone(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
            throw new InvalidOperationException("Clinic time zone is not configured.");

        return TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
    }

    private static WorkingSchedule CreateWorkingSchedule(WorkingHoursSettings settings)
        => new(
            settings.Start,
            settings.End,
            settings.BreakStart,
            settings.BreakEnd,
            settings.SlotMinutes,
            settings.WorkingDays.ToHashSet());
}
