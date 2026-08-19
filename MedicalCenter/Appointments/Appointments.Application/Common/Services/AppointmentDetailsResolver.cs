using Appointments.Application.Common.Dtos;
using Appointments.Application.Common.Interfaces;

namespace Appointments.Application.Common.Services;

public sealed class AppointmentDetailsResolver
{
    private readonly IDoctorDirectoryClient _doctorDirectoryClient;
    private readonly IServiceCatalogClient _serviceCatalogClient;
    private readonly IPatientDirectoryClient _patientDirectoryClient;

    public AppointmentDetailsResolver(
        IDoctorDirectoryClient  doctorDirectoryClient,
        IServiceCatalogClient   serviceCatalogClient,
        IPatientDirectoryClient patientDirectoryClient)
    {
        _doctorDirectoryClient  = doctorDirectoryClient;
        _serviceCatalogClient   = serviceCatalogClient;
        _patientDirectoryClient = patientDirectoryClient;
    }

    public async Task<IReadOnlyList<AppointmentListItemDto>> ResolveAsync(
        IReadOnlyList<AppointmentRowDto> rows,
        bool includePatients,
        CancellationToken cancellationToken = default)
    {
        if (rows.Count == 0)
            return [];

        var doctorIds  = rows.Select(row => row.DoctorId).Distinct().ToList();
        var serviceIds = rows.Select(row => row.ServiceId).Distinct().ToList();

        var doctorsTask  = _doctorDirectoryClient.GetSummariesAsync(doctorIds, cancellationToken);
        var servicesTask = _serviceCatalogClient.GetSummariesAsync(serviceIds, cancellationToken);

        await Task.WhenAll(doctorsTask, servicesTask);

        var doctors = (await doctorsTask).ToDictionary(doctor => doctor.Id);
        var services = (await servicesTask).ToDictionary(service => service.Id);

        var patients = new Dictionary<Guid, PatientSummaryDto>();

        if (includePatients)
        {
            var patientIds = rows.Select(row => row.PatientId).Distinct().ToList();

            patients = (await _patientDirectoryClient.GetSummariesAsync(patientIds, cancellationToken))
                .ToDictionary(patient => patient.Id);
        }

        return rows.Select(row =>
        {
            doctors.TryGetValue(row.DoctorId, out var doctor);
            services.TryGetValue(row.ServiceId, out var service);
            patients.TryGetValue(row.PatientId, out var patient);

            return new AppointmentListItemDto(
                row.Id,
                row.Date,
                row.StartTime,
                row.EndTime,
                row.DoctorId,
                doctor?.FirstName ?? string.Empty,
                doctor?.LastName ?? string.Empty,
                doctor?.MiddleName,
                row.PatientId,
                patient?.FirstName,
                patient?.LastName,
                patient?.MiddleName,
                patient?.PhoneNumber,
                row.ServiceId,
                service?.Name ?? string.Empty,
                row.Status);
        }).ToList();
    }
}
