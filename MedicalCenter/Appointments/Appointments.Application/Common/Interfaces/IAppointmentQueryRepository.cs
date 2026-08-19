using Appointments.Application.Common.Dtos;

namespace Appointments.Application.Common.Interfaces;

public interface IAppointmentQueryRepository
{
    Task<IReadOnlyList<BusyIntervalDto>> GetBusyIntervalsAsync(
        DateOnly date,
        IReadOnlyCollection<Guid> doctorIds,
        CancellationToken cancellationToken = default);

    Task<AppointmentRowDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AppointmentRowDto>> SearchAsync(
        AppointmentFilter filter,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AppointmentRowDto>> GetByDoctorAndDateAsync(
        Guid doctorId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AppointmentRowDto>> GetByPatientAsync(
        Guid patientId,
        CancellationToken cancellationToken = default);

    Task<bool> HasApprovedWithPatientAsync(
        Guid doctorId,
        Guid patientId,
        CancellationToken cancellationToken = default);

    Task<bool> HasOverlapAsync(
        Guid doctorId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        Guid? excludingAppointmentId,
        CancellationToken cancellationToken = default);
}
