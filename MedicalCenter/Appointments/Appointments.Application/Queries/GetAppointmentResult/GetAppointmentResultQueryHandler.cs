using Appointments.Application.Common.Dtos;
using Appointments.Application.Common.Interfaces;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;

namespace Appointments.Application.Queries.GetAppointmentResult;

public sealed class GetAppointmentResultQueryHandler
    : IRequestHandler<GetAppointmentResultQuery, ErrorOr<AppointmentResultDto>>
{
    private readonly IAppointmentQueryRepository _appointmentRepository;
    private readonly IAppointmentResultQueryRepository _resultRepository;
    private readonly IDoctorDirectoryClient _doctorDirectoryClient;
    private readonly IPatientDirectoryClient _patientDirectoryClient;
    private readonly IServiceCatalogClient _serviceCatalogClient;
    private readonly ICurrentUserProvider _currentUserProvider;

    public GetAppointmentResultQueryHandler(
        IAppointmentQueryRepository appointmentRepository,
        IAppointmentResultQueryRepository resultRepository,
        IDoctorDirectoryClient doctorDirectoryClient,
        IPatientDirectoryClient patientDirectoryClient,
        IServiceCatalogClient serviceCatalogClient,
        ICurrentUserProvider currentUserProvider)
    {
        _appointmentRepository = appointmentRepository;
        _resultRepository = resultRepository;
        _doctorDirectoryClient = doctorDirectoryClient;
        _patientDirectoryClient = patientDirectoryClient;
        _serviceCatalogClient = serviceCatalogClient;
        _currentUserProvider = currentUserProvider;
    }

    public async Task<ErrorOr<AppointmentResultDto>> Handle(
        GetAppointmentResultQuery request,
        CancellationToken cancellationToken)
    {
        var profileId = _currentUserProvider.User?.ProfileId;
        if (profileId is null)
            return Error.Validation("Profile.Required", "Profile was not found.");

        var appointment = await _appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken);
        if (appointment is null)
            return Error.NotFound("Appointment.NotFound", "Appointment was not found.");

        var isPatient = appointment.PatientId == profileId.Value;

        var isTreatingDoctor = !isPatient && await _appointmentRepository.HasApprovedWithPatientAsync(
            profileId.Value,
            appointment.PatientId,
            cancellationToken);

        if (!isPatient && !isTreatingDoctor)
            return Error.Forbidden("Appointment.Forbidden", "You are not allowed to perform this action.");

        var result = await _resultRepository.GetByAppointmentIdAsync(request.AppointmentId, cancellationToken);

        var doctorsTask = _doctorDirectoryClient.GetSummariesAsync([appointment.DoctorId], cancellationToken);
        var servicesTask = _serviceCatalogClient.GetSummariesAsync([appointment.ServiceId], cancellationToken);

        var patientsTask = isPatient
            ? Task.FromResult<IReadOnlyList<PatientSummaryDto>>([])
            : _patientDirectoryClient.GetSummariesAsync([appointment.PatientId], cancellationToken);

        await Task.WhenAll(doctorsTask, servicesTask, patientsTask);

        var doctor = (await doctorsTask).FirstOrDefault();
        var service = (await servicesTask).FirstOrDefault();
        var patient = (await patientsTask).FirstOrDefault();

        return new AppointmentResultDto(
            result?.Id,
            appointment.Id,
            appointment.Date,
            appointment.StartTime,
            appointment.EndTime,
            appointment.PatientId,
            patient?.FirstName ?? string.Empty,
            patient?.LastName ?? string.Empty,
            patient?.MiddleName,
            patient?.DateOfBirth ?? default,
            appointment.DoctorId,
            doctor?.FirstName ?? string.Empty,
            doctor?.LastName ?? string.Empty,
            doctor?.MiddleName,
            service?.SpecializationName ?? string.Empty,
            appointment.ServiceId,
            service?.Name ?? string.Empty,
            result?.Complaints,
            result?.Conclusion,
            result?.Recommendations,
            result?.Diagnosis);
    }
}
