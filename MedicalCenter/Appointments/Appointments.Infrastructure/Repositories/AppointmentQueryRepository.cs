using Appointments.Application.Common.Dtos;
using Appointments.Application.Common.Interfaces;
using Appointments.Domain.Enums;
using Appointments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Appointments.Infrastructure.Repositories;

public sealed class AppointmentQueryRepository : IAppointmentQueryRepository
{
    private readonly AppointmentsDbContext _context;

    public AppointmentQueryRepository(AppointmentsDbContext context)
    {
        _context = context;
    }

    public Task<bool> HasOverlapAsync(
        Guid doctorId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        Guid? excludingAppointmentId,
        CancellationToken cancellationToken = default)
    {
        var cancelled = AppointmentStatus.Cancelled.ToString();

        var query = _context.Appointments
            .AsNoTracking()
            .Where(appointment =>
                appointment.DoctorId == doctorId
                && appointment.Date == date
                && appointment.Status != cancelled
                && appointment.StartTime < endTime
                && startTime < appointment.EndTime);

        if (excludingAppointmentId.HasValue)
            query = query.Where(appointment => appointment.Id != excludingAppointmentId.Value);

        return query.AnyAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BusyIntervalDto>> GetBusyIntervalsAsync(
        DateOnly date,
        IReadOnlyCollection<Guid> doctorIds,
        CancellationToken cancellationToken = default)
    {
        if (doctorIds.Count == 0)
            return [];

        var cancelled = AppointmentStatus.Cancelled.ToString();

        return await _context.Appointments
            .AsNoTracking()
            .Where(appointment =>
                appointment.Date == date
                && doctorIds.Contains(appointment.DoctorId)
                && appointment.Status != cancelled)
            .Select(appointment => new BusyIntervalDto(
                appointment.DoctorId,
                appointment.StartTime,
                appointment.EndTime))
            .ToListAsync(cancellationToken);
    }
}
