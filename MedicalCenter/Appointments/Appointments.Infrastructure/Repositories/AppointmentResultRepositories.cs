using Appointments.Application.Common.Dtos;
using Appointments.Application.Common.Interfaces;
using Appointments.Domain;
using Appointments.Infrastructure.Persistence;
using Appointments.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Appointments.Infrastructure.Repositories;

public sealed class AppointmentResultCommandRepository : IAppointmentResultCommandRepository
{
    private readonly AppointmentsDbContext _context;

    public AppointmentResultCommandRepository(AppointmentsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AppointmentResult result, CancellationToken cancellationToken = default)
    {
        await _context.AppointmentResults.AddAsync(result.ToEntity(), cancellationToken);
    }

    public async Task<AppointmentResult?> GetByAppointmentIdAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.AppointmentResults
            .AsNoTracking()
            .FirstOrDefaultAsync(result => result.AppointmentId == appointmentId, cancellationToken);

        return entity?.ToDomain();
    }

    public void Update(AppointmentResult result, long expectedVersion)
    {
        var entry = _context.AppointmentResults.Attach(result.ToEntity());

        entry.State = EntityState.Modified;
        entry.Property(entity => entity.Version).OriginalValue = expectedVersion;
    }
}

public sealed class AppointmentResultQueryRepository : IAppointmentResultQueryRepository
{
    private readonly AppointmentsDbContext _context;

    public AppointmentResultQueryRepository(AppointmentsDbContext context)
    {
        _context = context;
    }

    public Task<AppointmentResultRowDto?> GetByAppointmentIdAsync(
        Guid appointmentId,
        CancellationToken cancellationToken = default)
        => _context.AppointmentResults
            .AsNoTracking()
            .Where(result => result.AppointmentId == appointmentId)
            .Select(result => new AppointmentResultRowDto(
                result.Id,
                result.AppointmentId,
                result.Complaints,
                result.Conclusion,
                result.Recommendations,
                result.Diagnosis))
            .FirstOrDefaultAsync(cancellationToken)!;

    public Task<bool> ExistsForAppointmentAsync(Guid appointmentId, CancellationToken cancellationToken = default)
        => _context.AppointmentResults
            .AsNoTracking()
            .AnyAsync(result => result.AppointmentId == appointmentId, cancellationToken);
}
