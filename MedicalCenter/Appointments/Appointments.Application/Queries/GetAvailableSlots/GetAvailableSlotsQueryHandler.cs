using Appointments.Application.Common.Dtos;
using Appointments.Application.Common.Interfaces;
using Appointments.Application.Common.Settings;
using Appointments.Domain.Scheduling;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Options;

namespace Appointments.Application.Queries.GetAvailableSlots;

public sealed class GetAvailableSlotsQueryHandler
    : IRequestHandler<GetAvailableSlotsQuery, ErrorOr<IReadOnlyList<AvailableSlotDto>>>
{
    private static readonly IReadOnlyList<AvailableSlotDto> NoSlots = [];

    private readonly IAppointmentQueryRepository _repository;
    private readonly IServiceCatalogClient _serviceCatalogClient;
    private readonly IDoctorDirectoryClient _doctorDirectoryClient;
    private readonly WorkingSchedule _schedule;
    private readonly WorkingHoursSettings _settings;
    private readonly TimeProvider _timeProvider;

    public GetAvailableSlotsQueryHandler(
        IAppointmentQueryRepository repository,
        IServiceCatalogClient serviceCatalogClient,
        IDoctorDirectoryClient doctorDirectoryClient,
        WorkingSchedule schedule,
        IOptions<WorkingHoursSettings> settings,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _serviceCatalogClient = serviceCatalogClient;
        _doctorDirectoryClient = doctorDirectoryClient;
        _schedule = schedule;
        _settings = settings.Value;
        _timeProvider = timeProvider;
    }

    public async Task<ErrorOr<IReadOnlyList<AvailableSlotDto>>> Handle(
        GetAvailableSlotsQuery request,
        CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow();
        var today = DateOnly.FromDateTime(now.UtcDateTime);

        if (request.Date < today)
            return Error.Validation("Slots.DateInPast", "Please, select the date");

        if (request.Date > today.AddDays(_settings.BookingHorizonDays))
            return Error.Validation(
                "Slots.DateBeyondHorizon",
                $"Appointments can be booked up to {_settings.BookingHorizonDays} days ahead.");

        var service = await _serviceCatalogClient.GetServiceAsync(request.ServiceId, cancellationToken);
        if (service is null)
            return Error.NotFound("Service.NotFound", "Service was not found.");

        if (!service.IsActive)
            return Error.Validation("Service.Inactive", "Please, choose the service");

        if (!_schedule.IsWorkingDay(request.Date))
            return ErrorOrFactory.From(NoSlots);

        var doctorIdsResult = await ResolveDoctorsAsync(request, service, cancellationToken);
        if (doctorIdsResult.IsError)
            return doctorIdsResult.Errors;

        var doctorIds = doctorIdsResult.Value;
        if (doctorIds.Count == 0)
            return ErrorOrFactory.From(NoSlots);

        var busy = await _repository.GetBusyIntervalsAsync(request.Date, doctorIds, cancellationToken);

        var busyByDoctor = busy
            .GroupBy(interval => interval.DoctorId)
            .ToDictionary(group => group.Key, group => group.ToList());

        var earliestStart = request.Date == today
            ? TimeOnly.FromDateTime(now.UtcDateTime)
            : (TimeOnly?)null;

        var slots = new List<AvailableSlotDto>();

        foreach (var startTime in _schedule.EnumerateStarts(service.DurationMinutes))
        {
            if (earliestStart.HasValue && startTime < earliestStart.Value)
                continue;

            var endTime = startTime.AddMinutes(service.DurationMinutes);

            var freeDoctors = doctorIds
                .Where(doctorId => IsFree(busyByDoctor, doctorId, startTime, endTime))
                .ToList();

            if (freeDoctors.Count > 0)
                slots.Add(new AvailableSlotDto(startTime, freeDoctors));
        }

        return ErrorOrFactory.From<IReadOnlyList<AvailableSlotDto>>(slots);
    }

    private async Task<ErrorOr<IReadOnlyList<Guid>>> ResolveDoctorsAsync(
        GetAvailableSlotsQuery request,
        ServiceForAppointmentDto service,
        CancellationToken cancellationToken)
    {
        if (!request.DoctorId.HasValue)
        {
            var ids = await _doctorDirectoryClient.GetAtWorkDoctorIdsAsync(
                service.SpecializationId,
                request.OfficeId,
                cancellationToken);

            return ErrorOrFactory.From(ids);
        }

        var doctor = await _doctorDirectoryClient.GetDoctorAsync(request.DoctorId.Value, cancellationToken);
        if (doctor is null)
            return Error.NotFound("Doctor.NotFound", "Doctor was not found.");

        if (!doctor.IsAtWork)
            return Error.Validation("Doctor.NotAtWork", "Please, choose the doctor");

        if (doctor.SpecializationId != service.SpecializationId)
            return Error.Validation("Doctor.SpecializationMismatch", "Please, choose the doctor");

        if (request.OfficeId.HasValue && doctor.OfficeId != request.OfficeId.Value)
            return Error.Validation("Doctor.OfficeMismatch", "Please, choose the office");

        return ErrorOrFactory.From<IReadOnlyList<Guid>>([doctor.Id]);
    }

    private static bool IsFree(
        IReadOnlyDictionary<Guid, List<BusyIntervalDto>> busyByDoctor,
        Guid doctorId,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        if (!busyByDoctor.TryGetValue(doctorId, out var intervals))
            return true;

        return intervals.All(interval => startTime >= interval.EndTime || interval.StartTime >= endTime);
    }
}
