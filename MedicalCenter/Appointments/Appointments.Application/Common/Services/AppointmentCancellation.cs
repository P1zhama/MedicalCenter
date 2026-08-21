using Appointments.Application.Common.Interfaces;
using Appointments.Domain;
using Appointments.Domain.Constants;
using Microsoft.Extensions.Logging;

namespace Appointments.Application.Common.Services;

public sealed class AppointmentCancellation
{
    private readonly IAppointmentCommandRepository _repository;
    private readonly ClinicClock _clock;
    private readonly AppointmentNotifier _notifier;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<AppointmentCancellation> _logger;

    public AppointmentCancellation(
        IAppointmentCommandRepository repository,
        ClinicClock clock,
        AppointmentNotifier notifier,
        TimeProvider timeProvider,
        ILogger<AppointmentCancellation> logger)
    {
        _repository = repository;
        _clock = clock;
        _notifier = notifier;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<int> CancelUpcomingByDoctorAsync(Guid doctorId, CancellationToken cancellationToken = default)
    {
        var appointments = await _repository.GetUpcomingByDoctorAsync(
            doctorId,
            _clock.Today,
            _clock.CurrentTime,
            cancellationToken);

        return await CancelAsync(appointments, cancellationToken);
    }

    public async Task<int> CancelUpcomingByServiceAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        var appointments = await _repository.GetUpcomingByServiceAsync(
            serviceId,
            _clock.Today,
            _clock.CurrentTime,
            cancellationToken);

        return await CancelAsync(appointments, cancellationToken);
    }

    private async Task<int> CancelAsync(
        IReadOnlyList<Appointment> appointments,
        CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow();
        var cancelled = new List<Appointment>(appointments.Count);

        foreach (var appointment in appointments)
        {
            if (appointment.IsCancelled)
                continue;

            var expectedVersion = appointment.Version;

            appointment.Cancel(SystemActors.Cascade, now);
            _repository.Update(appointment, expectedVersion);

            cancelled.Add(appointment);
        }

        if (cancelled.Count == 0)
            return 0;

        _logger.LogInformation("Cancelled {Count} upcoming appointment(s) by cascade.", cancelled.Count);

        await _notifier.NotifyAsync(cancelled, AppointmentNotificationKinds.Cancelled, cancellationToken);

        return cancelled.Count;
    }
}
