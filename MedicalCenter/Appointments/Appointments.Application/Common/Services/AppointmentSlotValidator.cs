using Appointments.Application.Common.Interfaces;
using Appointments.Application.Common.Settings;
using Appointments.Domain.Scheduling;
using ErrorOr;
using Microsoft.Extensions.Options;

namespace Appointments.Application.Common.Services;

public sealed class AppointmentSlotValidator
{
    private readonly IServiceCatalogClient _serviceCatalogClient;
    private readonly IDoctorDirectoryClient _doctorDirectoryClient;
    private readonly IAppointmentQueryRepository _repository;
    private readonly WorkingSchedule _schedule;
    private readonly WorkingHoursSettings _settings;
    private readonly ClinicClock _clock;

    public AppointmentSlotValidator(
        IServiceCatalogClient serviceCatalogClient,
        IDoctorDirectoryClient doctorDirectoryClient,
        IAppointmentQueryRepository repository,
        WorkingSchedule schedule,
        IOptions<WorkingHoursSettings> settings,
        ClinicClock clock)
    {
        _serviceCatalogClient = serviceCatalogClient;
        _doctorDirectoryClient = doctorDirectoryClient;
        _repository = repository;
        _schedule = schedule;
        _settings = settings.Value;
        _clock = clock;
    }

    public async Task<ErrorOr<int>> ValidateAsync(
        Guid serviceId,
        Guid doctorId,
        Guid officeId,
        DateOnly date,
        TimeOnly startTime,
        Guid? excludingAppointmentId,
        CancellationToken cancellationToken = default)
    {
        var today = _clock.Today;

        if (date < today)
            return Error.Validation("Appointment.DateInPast", "Please, select the date");

        if (date > today.AddDays(_settings.BookingHorizonDays))
            return Error.Validation(
                "Appointment.DateBeyondHorizon",
                $"Appointments can be booked up to {_settings.BookingHorizonDays} days ahead.");

        if (!_schedule.IsWorkingDay(date))
            return Error.Validation("Appointment.NotWorkingDay", "Please, select the date");

        if (date == today && startTime < _clock.CurrentTime)
            return Error.Validation("Appointment.TimeInPast", "Please, select the time slot");

        var service = await _serviceCatalogClient.GetServiceAsync(serviceId, cancellationToken);
        if (service is null)
            return Error.NotFound("Service.NotFound", "Service was not found.");

        if (!service.IsActive)
            return Error.Validation("Service.Inactive", "Please, choose the service");

        if (!_schedule.Fits(startTime, service.DurationMinutes))
            return Error.Validation("Appointment.SlotOutsideWorkingHours", "Please, select the time slot");

        var doctor = await _doctorDirectoryClient.GetDoctorAsync(doctorId, cancellationToken);
        if (doctor is null)
            return Error.NotFound("Doctor.NotFound", "Doctor was not found.");

        if (!doctor.IsAtWork)
            return Error.Validation("Doctor.NotAtWork", "Please, choose the doctor");

        if (doctor.SpecializationId != service.SpecializationId)
            return Error.Validation("Doctor.SpecializationMismatch", "Please, choose the doctor");

        if (doctor.OfficeId != officeId)
            return Error.Validation("Appointment.OfficeMismatch", "Please, choose the office");

        var endTime = startTime.AddMinutes(service.DurationMinutes);

        var overlaps = await _repository.HasOverlapAsync(
            doctorId,
            date,
            startTime,
            endTime,
            excludingAppointmentId,
            cancellationToken);

        if (overlaps)
            return Error.Conflict("Appointment.SlotTaken", "This time slot is already taken. Please, select another one.");

        return service.DurationMinutes;
    }
}
