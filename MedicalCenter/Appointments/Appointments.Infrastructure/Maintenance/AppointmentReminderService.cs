using Appointments.Application.Common.Interfaces;
using Appointments.Application.Common.Services;
using Appointments.Application.Common.Settings;
using Common.Abstractions.Eventing;
using MedicalCenter.Shared.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Appointments.Infrastructure.Maintenance;

public sealed class AppointmentReminderService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ReminderSettings _settings;
    private readonly ILogger<AppointmentReminderService> _logger;

    public AppointmentReminderService(
        IServiceScopeFactory scopeFactory,
        IOptions<ReminderSettings> settings,
        ILogger<AppointmentReminderService> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Appointment reminders are disabled.");

            return;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_settings.IntervalMinutes));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await SendAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Sending appointment reminders failed; nothing was marked this cycle.");
            }
        }
    }

    private async Task SendAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var repository = scope.ServiceProvider.GetRequiredService<IAppointmentCommandRepository>();
        var doctors = scope.ServiceProvider.GetRequiredService<IDoctorDirectoryClient>();
        var services = scope.ServiceProvider.GetRequiredService<IServiceCatalogClient>();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var clock = scope.ServiceProvider.GetRequiredService<ClinicClock>();

        var tomorrow = clock.Today.AddDays(1);

        var due = await repository.GetDueRemindersAsync(tomorrow, _settings.BatchSize, cancellationToken);

        if (due.Count == 0)
            return;

        var doctorSummaries = (await doctors.GetSummariesAsync(
            due.Select(appointment => appointment.DoctorId).Distinct().ToList(),
            cancellationToken)).ToDictionary(doctor => doctor.Id);

        var serviceSummaries = (await services.GetSummariesAsync(
            due.Select(appointment => appointment.ServiceId).Distinct().ToList(),
            cancellationToken)).ToDictionary(service => service.Id);

        var now = DateTimeOffset.UtcNow;

        foreach (var appointment in due)
        {
            doctorSummaries.TryGetValue(appointment.DoctorId, out var doctor);
            serviceSummaries.TryGetValue(appointment.ServiceId, out var service);

            await publisher.PublishAsync(
                new AppointmentNotificationRequested(
                    appointment.PatientId,
                    AppointmentNotificationKinds.Reminder,
                    appointment.Date.ToDateTime(appointment.StartTime),
                    appointment.PatientFullName,
                    PatientNames.Full(doctor?.LastName, doctor?.FirstName, doctor?.MiddleName),
                    service?.Name ?? string.Empty),
                cancellationToken);

            repository.MarkReminderSent(appointment.Id, appointment.Version, now);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Queued {Count} reminder(s) for appointments on {Date}.",
            due.Count,
            tomorrow);
    }
}
