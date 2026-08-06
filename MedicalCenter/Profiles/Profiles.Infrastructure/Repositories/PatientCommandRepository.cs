using Microsoft.EntityFrameworkCore;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain;
using Profiles.Infrastructure.Persistence;
using Profiles.Infrastructure.Persistence.Entities;
using Profiles.Infrastructure.Persistence.Mappers;

namespace Profiles.Infrastructure.Repositories;

public sealed class PatientCommandRepository : IPatientCommandRepository
{
    private readonly ProfilesDbContext _context;

    public PatientCommandRepository(ProfilesDbContext context)
    {
        _context = context;
    }

    public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(patient => patient.Id == id, cancellationToken);

        return entity?.ToDomain();
    }

    public async Task<IReadOnlyList<Patient>> GetMatchCandidatesAsync(
        string firstName,
        string lastName,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.Patients
            .AsNoTracking()
            .Where(patient => patient.AccountId == null)
            .Where(patient => patient.FirstName == firstName || patient.LastName == lastName)
            .ToListAsync(cancellationToken);

        return entities.Select(entity => entity.ToDomain()).ToList();
    }

    public async Task AddAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        await _context.Patients.AddAsync(patient.ToEntity(), cancellationToken);
    }

    public void Update(Patient patient, long expectedVersion)
    {
        var entry = _context.Patients.Attach(patient.ToEntity());

        entry.State = EntityState.Modified;
        entry.Property(entity => entity.Version).OriginalValue = expectedVersion;
    }

    public void Remove(Guid id)
    {
        _context.Patients.Remove(new PatientEntity { Id = id });
    }
}
