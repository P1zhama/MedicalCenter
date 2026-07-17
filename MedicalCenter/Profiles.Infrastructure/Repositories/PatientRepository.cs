using Microsoft.EntityFrameworkCore;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain;
using Profiles.Infrastructure.Persistence;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Profiles.Infrastructure.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly ApplicationDbContext _context;

    public PatientRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AddAsync(Patient patient, CancellationToken cancellationToken)
    {
        await _context.Patients.AddAsync(patient, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Patient patient, CancellationToken cancellationToken)
    {
        _context.Patients.Update(patient);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Patient?> GetBestMatchAsync(
        string firstName,
        string lastName,
        string? middleName,
        DateOnly dateOfBirth,
        CancellationToken cancellationToken)
    {
        var candidates = await _context.Patients
            .AsNoTracking()
            .Where(p => p.AccountId == null)
            .Where(p => p.FirstName == firstName || p.LastName == lastName)
            .ToListAsync(cancellationToken);

        Patient? bestMatch = null;
        var bestWeight = 0;

        foreach (var candidate in candidates)
        {
            var weight = 0;

            if (string.Equals(candidate.FirstName, firstName, StringComparison.OrdinalIgnoreCase))
                weight += 5;

            if (string.Equals(candidate.LastName, lastName, StringComparison.OrdinalIgnoreCase))
                weight += 5;

            if (!string.IsNullOrEmpty(candidate.MiddleName) &&
                string.Equals(candidate.MiddleName, middleName, StringComparison.OrdinalIgnoreCase))
                weight += 5;

            if (candidate.DateOfBirth == dateOfBirth)
                weight += 3;

            if (weight >= 13 && weight > bestWeight)
            {
                bestWeight = weight;
                bestMatch = candidate;
            }
        }

        return bestMatch;
    }
}
