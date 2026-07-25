using Microsoft.EntityFrameworkCore;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain;
using Profiles.Infrastructure.Persistence;
using Profiles.Infrastructure.Persistence.Mappers;

namespace Profiles.Infrastructure.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly ApplicationDbContext _context;

    public PatientRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Patients
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

    public async Task UpdateAsync(Patient patient, CancellationToken cancellationToken = default)
    {
        var tracked = await _context.Patients.FindAsync([patient.Id], cancellationToken);
        if (tracked is null)
            throw new InvalidOperationException($"Patient {patient.Id} must be loaded before update.");

        _context.Entry(tracked).CurrentValues.SetValues(patient.ToEntity());
    }
}
