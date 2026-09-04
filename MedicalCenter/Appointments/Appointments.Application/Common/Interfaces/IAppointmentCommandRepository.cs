using Appointments.Application.Common.Dtos;
using Appointments.Domain;

namespace Appointments.Application.Common.Interfaces;

public interface IAppointmentCommandRepository
{
    Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default);

    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AppointmentReminderDto>> GetDueRemindersAsync(
        DateOnly date,
        int limit,
        CancellationToken cancellationToken = default);

    void MarkReminderSent(Guid appointmentId, long expectedVersion, DateTimeOffset sentAt);

    void Update(Appointment appointment, long expectedVersion);

    Task<IReadOnlyList<Appointment>> GetUpcomingByDoctorAsync(
        Guid doctorId,
        DateOnly fromDate,
        TimeOnly fromTime,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Appointment>> GetUpcomingByServiceAsync(
        Guid serviceId,
        DateOnly fromDate,
        TimeOnly fromTime,
        CancellationToken cancellationToken = default);
}
