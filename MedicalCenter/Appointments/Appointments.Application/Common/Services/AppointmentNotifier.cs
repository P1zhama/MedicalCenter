using Appointments.Application.Common.Interfaces;
using Appointments.Domain;
using Common.Abstractions.Eventing;
using MedicalCenter.Shared.Contracts;
using Microsoft.Extensions.Logging;

namespace Appointments.Application.Common.Services;

public static class AppointmentNotificationKinds
{
    public const string Approved = "Approved";

    public const string Cancelled = "Cancelled";

    public const string Rescheduled = "Rescheduled";

    public const string Reminder = "Reminder";
}

public sealed class AppointmentNotifier
{
    private readonly IDoctorDirectoryClient _doctorDirectoryClient;
    private readonly IServiceCatalogClient _serviceCatalogClient;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<AppointmentNotifier> _logger;

    public AppointmentNotifier(
        IDoctorDirectoryClient doctorDirectoryClient,
        IServiceCatalogClient serviceCatalogClient,
        IEventPublisher eventPublisher,
        ILogger<AppointmentNotifier> logger)
    {
        _doctorDirectoryClient = doctorDirectoryClient;
        _serviceCatalogClient = serviceCatalogClient;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public Task NotifyAsync(Appointment appointment, string kind, CancellationToken cancellationToken = default)
        => NotifyAsync([appointment], kind, cancellationToken);

    public async Task NotifyAsync(
        IReadOnlyList<Appointment> appointments,
        string kind,
        CancellationToken cancellationToken = default)
    {
        if (appointments.Count == 0)
            return;

        try
        {
            var doctorIds = appointments.Select(appointment => appointment.DoctorId).Distinct().ToList();
            var serviceIds = appointments.Select(appointment => appointment.ServiceId).Distinct().ToList();

            var doctorsTask = _doctorDirectoryClient.GetSummariesAsync(doctorIds, cancellationToken);
            var servicesTask = _serviceCatalogClient.GetSummariesAsync(serviceIds, cancellationToken);

            await Task.WhenAll(doctorsTask, servicesTask);

            var doctors = (await doctorsTask).ToDictionary(doctor => doctor.Id);
            var services = (await servicesTask).ToDictionary(service => service.Id);

            foreach (var appointment in appointments)
            {
                doctors.TryGetValue(appointment.DoctorId, out var doctor);
                services.TryGetValue(appointment.ServiceId, out var service);

                await _eventPublisher.PublishAsync(
                    new AppointmentNotificationRequested(
                        appointment.PatientId,
                        kind,
                        appointment.Date.ToDateTime(appointment.StartTime),
                        appointment.PatientFullName,
                        FullName(doctor?.LastName, doctor?.FirstName, doctor?.MiddleName),
                        service?.Name ?? string.Empty),
                    cancellationToken);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Could not queue {Kind} notification for {Count} appointment(s); the change itself is not affected.",
                kind,
                appointments.Count);
        }
    }

    private static string FullName(string? lastName, string? firstName, string? middleName)
        => string.Join(' ', new[] { lastName, firstName, middleName }
            .Where(part => !string.IsNullOrWhiteSpace(part)));
}
