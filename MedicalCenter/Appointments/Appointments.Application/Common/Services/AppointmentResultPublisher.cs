using Appointments.Application.Common.Dtos;
using Appointments.Application.Common.Interfaces;
using Common.Abstractions.Eventing;
using MedicalCenter.Shared.Contracts;
using Microsoft.Extensions.Logging;

namespace Appointments.Application.Common.Services;

public sealed class AppointmentResultPublisher
{
    private readonly IDoctorDirectoryClient _doctorDirectoryClient;
    private readonly IServiceCatalogClient _serviceCatalogClient;
    private readonly IPatientDirectoryClient _patientDirectoryClient;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<AppointmentResultPublisher> _logger;

    public AppointmentResultPublisher(
        IDoctorDirectoryClient doctorDirectoryClient,
        IServiceCatalogClient serviceCatalogClient,
        IPatientDirectoryClient patientDirectoryClient,
        IEventPublisher eventPublisher,
        ILogger<AppointmentResultPublisher> logger)
    {
        _doctorDirectoryClient = doctorDirectoryClient;
        _serviceCatalogClient = serviceCatalogClient;
        _patientDirectoryClient = patientDirectoryClient;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task PublishReadyAsync(
        AppointmentRowDto appointment,
        string complaints,
        string conclusion,
        string? diagnosis,
        string recommendations,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var doctorsTask = _doctorDirectoryClient.GetSummariesAsync([appointment.DoctorId], cancellationToken);
            var servicesTask = _serviceCatalogClient.GetSummariesAsync([appointment.ServiceId], cancellationToken);
            var patientsTask = _patientDirectoryClient.GetSummariesAsync([appointment.PatientId], cancellationToken);

            await Task.WhenAll(doctorsTask, servicesTask, patientsTask);

            var doctor = (await doctorsTask).FirstOrDefault();
            var service = (await servicesTask).FirstOrDefault();
            var patient = (await patientsTask).FirstOrDefault();

            await _eventPublisher.PublishAsync(
                new AppointmentResultReadyEvent(
                    appointment.Id,
                    appointment.PatientId,
                    appointment.Date.ToDateTime(appointment.StartTime),
                    appointment.Date.ToDateTime(appointment.EndTime),
                    FullName(patient?.LastName, patient?.FirstName, patient?.MiddleName),
                    patient?.DateOfBirth.ToDateTime(TimeOnly.MinValue) ?? default,
                    FullName(doctor?.LastName, doctor?.FirstName, doctor?.MiddleName),
                    service?.SpecializationName ?? string.Empty,
                    service?.Name ?? string.Empty,
                    complaints,
                    conclusion,
                    diagnosis,
                    recommendations),
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Could not queue the result of appointment {AppointmentId} for delivery; the result itself is saved.",
                appointment.Id);
        }
    }

    private static string FullName(string? lastName, string? firstName, string? middleName)
        => string.Join(' ', new[] { lastName, firstName, middleName }
            .Where(part => !string.IsNullOrWhiteSpace(part)));
}
