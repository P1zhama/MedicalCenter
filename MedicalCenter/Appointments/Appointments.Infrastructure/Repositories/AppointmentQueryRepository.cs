using Common.Abstractions.Paging;
using Appointments.Application.Common.Dtos;
using Appointments.Application.Common.Interfaces;
using Appointments.Domain.Enums;
using Appointments.Infrastructure.Persistence;
using Appointments.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Appointments.Infrastructure.Repositories;

public sealed class AppointmentQueryRepository : IAppointmentQueryRepository
{
    private readonly AppointmentsDbContext _context;

    public AppointmentQueryRepository(AppointmentsDbContext context)
    {
        _context = context;
    }

    private IQueryable<AppointmentEntity> NotCancelled()
    {
        var cancelled = AppointmentStatus.Cancelled.ToString();

        return _context.Appointments.AsNoTracking().Where(appointment => appointment.Status != cancelled);
    }

    private static IQueryable<AppointmentRowDto> Project(IQueryable<AppointmentEntity> query)
        => query.Select(appointment => new AppointmentRowDto(
            appointment.Id,
            appointment.Date,
            appointment.StartTime,
            appointment.EndTime,
            appointment.DoctorId,
            appointment.PatientId,
            appointment.ServiceId,
            appointment.OfficeId,
            appointment.Status));

    public Task<AppointmentRowDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Project(NotCancelled().Where(appointment => appointment.Id == id)).FirstOrDefaultAsync(cancellationToken)!;

    public async Task<IReadOnlyList<AppointmentRowDto>> SearchAsync(
        AppointmentFilter filter,
        CancellationToken cancellationToken = default)
    {
        var query = NotCancelled().Where(appointment => appointment.Date == filter.Date);

        if (filter.DoctorId.HasValue)
            query = query.Where(appointment => appointment.DoctorId == filter.DoctorId.Value);

        if (filter.ServiceId.HasValue)
            query = query.Where(appointment => appointment.ServiceId == filter.ServiceId.Value);

        if (filter.OfficeId.HasValue)
            query = query.Where(appointment => appointment.OfficeId == filter.OfficeId.Value);

        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(appointment => appointment.Status == filter.Status);

        return await Project(query.OrderBy(appointment => appointment.StartTime)).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AppointmentRowDto>> GetByDoctorAndDateAsync(
        Guid doctorId,
        DateOnly date,
        CancellationToken cancellationToken = default)
        => await Project(NotCancelled()
                .Where(appointment => appointment.DoctorId == doctorId && appointment.Date == date)
                .OrderBy(appointment => appointment.StartTime))
            .ToListAsync(cancellationToken);

    public async Task<PagedResult<AppointmentRowDto>> GetByPatientAsync(
        Guid patientId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = NotCancelled().Where(appointment => appointment.PatientId == patientId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await Project(query
                .OrderByDescending(appointment => appointment.Date)
                .ThenBy(appointment => appointment.StartTime)
                .ThenBy(appointment => appointment.Id)
                .Skip(PageRequest.Skip(page, pageSize))
                .Take(pageSize))
            .ToListAsync(cancellationToken);

        return new PagedResult<AppointmentRowDto>(items, page, pageSize, totalCount);
    }

    public Task<bool> HasApprovedWithPatientAsync(
        Guid doctorId,
        Guid patientId,
        CancellationToken cancellationToken = default)
    {
        var approved = AppointmentStatus.Approved.ToString();

        return _context.Appointments
            .AsNoTracking()
            .AnyAsync(
                appointment => appointment.DoctorId == doctorId
                    && appointment.PatientId == patientId
                    && appointment.Status == approved,
                cancellationToken);
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
