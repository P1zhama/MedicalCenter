using System.Linq.Expressions;
using Appointments.Application.Common.Dtos;
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

    public async Task<IReadOnlyList<AppointmentReminderDto>> GetDueRemindersAsync(
        DateOnly date,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var cancelled = AppointmentStatus.Cancelled.ToString();

        return await _context.Appointments
            .AsNoTracking()
            .Where(appointment => appointment.Date == date)
            .Where(appointment => appointment.Status != cancelled)
            .Where(appointment => appointment.ReminderSentAt == null)
            .OrderBy(appointment => appointment.StartTime)
            .Take(limit)
            .Select(appointment => new AppointmentReminderDto(
                appointment.Id,
                appointment.PatientId,
                appointment.PatientFullName,
                appointment.DoctorId,
                appointment.ServiceId,
                appointment.Date,
                appointment.StartTime,
                appointment.Version))
            .ToListAsync(cancellationToken);
    }

    public void MarkReminderSent(Guid appointmentId, long expectedVersion, DateTimeOffset sentAt)
    {
        var entry = _context.Appointments.Attach(new AppointmentEntity
        {
            Id = appointmentId,
            Version = expectedVersion,
            ReminderSentAt = sentAt
        });

        entry.Property(appointment => appointment.ReminderSentAt).IsModified = true;
    }

    public void Update(Appointment appointment, long expectedVersion)
    {
        var entry = _context.Appointments.Attach(appointment.ToEntity());

        entry.State = EntityState.Modified;
        entry.Property(entity => entity.Version).OriginalValue = expectedVersion;
    }
}
