using Microsoft.EntityFrameworkCore;
using Profiles.Application.Common.Interfaces;
using Profiles.Infrastructure.Persistence;

namespace Profiles.Infrastructure.Repositories;

public sealed class PhotoReferenceRepository : IPhotoReferenceRepository
{
    private readonly ProfilesDbContext _context;

    public PhotoReferenceRepository(ProfilesDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<string>> GetPhotoUrlsAsync(CancellationToken cancellationToken = default)
    {
        var doctors = _context.Doctors
            .AsNoTracking()
            .Where(doctor => doctor.PhotoUrl != null)
            .Select(doctor => doctor.PhotoUrl!);

        var patients = _context.Patients
            .AsNoTracking()
            .Where(patient => patient.PhotoUrl != null)
            .Select(patient => patient.PhotoUrl!);

        var receptionists = _context.Receptionists
            .AsNoTracking()
            .Where(receptionist => receptionist.PhotoUrl != null)
            .Select(receptionist => receptionist.PhotoUrl!);

        return await doctors
            .Concat(patients)
            .Concat(receptionists)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
