using Appointments.Application.Common.Dtos;

namespace Appointments.Application.Common.Interfaces;

public interface IAppointmentQueryRepository
{
    Task<IReadOnlyList<BusyIntervalDto>> GetBusyIntervalsAsync(
        DateOnly date,
        IReadOnlyCollection<Guid> doctorIds,
        CancellationToken cancellationToken = default);
}
