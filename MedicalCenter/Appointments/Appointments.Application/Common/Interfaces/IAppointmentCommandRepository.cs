using Appointments.Domain;

namespace Appointments.Application.Common.Interfaces;

public interface IAppointmentCommandRepository
{
    Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default);

    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Update(Appointment appointment, long expectedVersion);
}
