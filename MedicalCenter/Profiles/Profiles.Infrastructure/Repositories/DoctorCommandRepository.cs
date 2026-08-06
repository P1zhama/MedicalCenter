using Microsoft.EntityFrameworkCore;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain;
using Profiles.Infrastructure.Persistence;
using Profiles.Infrastructure.Persistence.Entities;
using Profiles.Infrastructure.Persistence.Mappers;

namespace Profiles.Infrastructure.Repositories;

public sealed class DoctorCommandRepository : IDoctorCommandRepository
{
    private readonly ProfilesDbContext _context;

    public DoctorCommandRepository(ProfilesDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Doctor doctor, CancellationToken cancellationToken = default)
    {
        await _context.Doctors.AddAsync(doctor.ToEntity(), cancellationToken);
    }

    public async Task<Doctor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(doctor => doctor.Id == id, cancellationToken);

        return entity?.ToDomain();
    }

    public async Task<IReadOnlyList<Doctor>> GetByOfficeAsync(Guid officeId, CancellationToken cancellationToken = default)
    {
        var entities = await _context.Doctors
            .AsNoTracking()
            .Where(doctor => doctor.OfficeId == officeId)
            .ToListAsync(cancellationToken);

        return entities.ConvertAll(entity => entity.ToDomain());
    }

    public async Task<IReadOnlyList<Doctor>> GetBySpecializationAsync(
        Guid specializationId,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.Doctors
            .AsNoTracking()
            .Where(doctor => doctor.SpecializationId == specializationId)
            .ToListAsync(cancellationToken);

        return entities.ConvertAll(entity => entity.ToDomain());
    }

    public void Update(Doctor doctor, long expectedVersion)
    {
        var entry = _context.Doctors.Attach(doctor.ToEntity());

        entry.State = EntityState.Modified;
        entry.Property(entity => entity.Version).OriginalValue = expectedVersion;
    }

    public void Remove(Guid id)
    {
        _context.Doctors.Remove(new DoctorEntity { Id = id });
    }
}
