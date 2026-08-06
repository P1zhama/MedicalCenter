using Microsoft.EntityFrameworkCore;
using Profiles.Application.Common.Dtos;
using Profiles.Application.Common.Interfaces;
using Profiles.Infrastructure.Persistence;

namespace Profiles.Infrastructure.Repositories;

public sealed class PatientQueryRepository : IPatientQueryRepository
{
    private readonly ProfilesDbContext _context;

    public PatientQueryRepository(ProfilesDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PatientListItemDto>> SearchAsync(
        string? fullNameSearch,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Patients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(fullNameSearch))
        {
            var search = fullNameSearch.Trim();

            query = query.Where(patient =>
                EF.Functions.Like(patient.FirstName, $"%{search}%")
                || EF.Functions.Like(patient.LastName, $"%{search}%")
                || (patient.MiddleName != null && EF.Functions.Like(patient.MiddleName, $"%{search}%")));
        }

        return await query
            .OrderBy(patient => patient.LastName)
            .ThenBy(patient => patient.FirstName)
            .Select(patient => new PatientListItemDto(
                patient.Id,
                patient.FirstName,
                patient.LastName,
                patient.MiddleName,
                patient.PhoneNumber))
            .ToListAsync(cancellationToken);
    }

    public Task<PatientDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Project(_context.Patients.AsNoTracking().Where(patient => patient.Id == id))
            .FirstOrDefaultAsync(cancellationToken)!;

    public Task<PatientDto?> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default)
        => Project(_context.Patients.AsNoTracking().Where(patient => patient.AccountId == accountId))
            .FirstOrDefaultAsync(cancellationToken)!;

    private static IQueryable<PatientDto> Project(IQueryable<Persistence.Entities.PatientEntity> query)
        => query.Select(patient => new PatientDto(
            patient.Id,
            patient.PhotoUrl,
            patient.FirstName,
            patient.LastName,
            patient.MiddleName,
            patient.PhoneNumber,
            patient.DateOfBirth,
            patient.AccountId != null));
}
