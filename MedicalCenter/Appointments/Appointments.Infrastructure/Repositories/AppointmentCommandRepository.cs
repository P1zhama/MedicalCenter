using Appointments.Application.Common.Interfaces;
using Appointments.Domain;
using Appointments.Infrastructure.Persistence;
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

    public void Update(Appointment appointment, long expectedVersion)
    {
        var entry = _context.Appointments.Attach(appointment.ToEntity());

        entry.State = EntityState.Modified;
        entry.Property(entity => entity.Version).OriginalValue = expectedVersion;
    }
}
