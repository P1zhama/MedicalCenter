using Appointments.Application.Common.Interfaces;
using Appointments.Application.Common.Settings;
using Appointments.Domain.Scheduling;
using Appointments.Infrastructure.Persistence;
using Appointments.Infrastructure.Repositories;
using Common.Infrastructure;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        services.Configure<WorkingHoursSettings>(configuration.GetSection(WorkingHoursSettings.SectionName));
        services.AddSingleton(CreateWorkingSchedule(workingHours));

        services.AddScoped<IAppointmentCommandRepository, AppointmentCommandRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddMassTransit(bus =>
        {
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

    private static WorkingSchedule CreateWorkingSchedule(WorkingHoursSettings settings)
        => new(
            settings.Start,
            settings.End,
            settings.BreakStart,
            settings.BreakEnd,
            settings.SlotMinutes,
            settings.WorkingDays.ToHashSet());
}
