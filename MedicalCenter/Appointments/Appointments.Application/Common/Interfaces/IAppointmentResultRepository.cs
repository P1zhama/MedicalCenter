using Appointments.Application.Common.Dtos;
using Appointments.Domain;

namespace Appointments.Application.Common.Interfaces;

public interface IAppointmentResultCommandRepository
{
    Task AddAsync(AppointmentResult result, CancellationToken cancellationToken = default);

    Task<AppointmentResult?> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken = default);

    void Update(AppointmentResult result, long expectedVersion);
}

public interface IAppointmentResultQueryRepository
{
    Task<AppointmentResultRowDto?> GetByAppointmentIdAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsForAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken = default);
}
