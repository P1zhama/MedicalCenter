using System.Linq.Expressions;
using Appointments.Application.Common.Interfaces;
using Appointments.Domain;
using Appointments.Domain.Enums;
using Appointments.Infrastructure.Persistence;
using Appointments.Infrastructure.Persistence.Entities;
using Appointments.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Appointments.Infrastructure.Repositories;

public sealed class AppointmentCommandRepository : IAppointmentCommandRepository
{
    private readonly AppointmentsDbContext _context;

    public AppointmentCommandRepository(AppointmentsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        await _context.Appointments.AddAsync(appointment.ToEntity(), cancellationToken);
    }

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Appointments
            .AsNoTracking()
            .FirstOrDefaultAsync(appointment => appointment.Id == id, cancellationToken);

        return entity?.ToDomain();
    }

    public Task<IReadOnlyList<Appointment>> GetUpcomingByDoctorAsync(
        Guid doctorId,
        DateOnly fromDate,
        TimeOnly fromTime,
        CancellationToken cancellationToken = default)
        => GetUpcomingAsync(
            appointment => appointment.DoctorId == doctorId,
            fromDate,
            fromTime,
            cancellationToken);

    public Task<IReadOnlyList<Appointment>> GetUpcomingByServiceAsync(
        Guid serviceId,
        DateOnly fromDate,
        TimeOnly fromTime,
        CancellationToken cancellationToken = default)
        => GetUpcomingAsync(
            appointment => appointment.ServiceId == serviceId,
            fromDate,
            fromTime,
            cancellationToken);

    private async Task<IReadOnlyList<Appointment>> GetUpcomingAsync(
        Expression<Func<AppointmentEntity, bool>> predicate,
        DateOnly fromDate,
        TimeOnly fromTime,
        CancellationToken cancellationToken)
    {
        var cancelled = AppointmentStatus.Cancelled.ToString();

        var entities = await _context.Appointments
            .AsNoTracking()
            .Where(predicate)
            .Where(appointment =>
                appointment.Status != cancelled
                && (appointment.Date > fromDate
                    || (appointment.Date == fromDate && appointment.StartTime > fromTime)))
            .ToListAsync(cancellationToken);

        return entities.ConvertAll(entity => entity.ToDomain());
    }

    public void Update(Appointment appointment, long expectedVersion)
    {
        var entry = _context.Appointments.Attach(appointment.ToEntity());

        entry.State = EntityState.Modified;
        entry.Property(entity => entity.Version).OriginalValue = expectedVersion;
    }
}
