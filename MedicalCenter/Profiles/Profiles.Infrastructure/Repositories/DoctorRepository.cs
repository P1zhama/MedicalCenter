using Microsoft.EntityFrameworkCore;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain;
using Profiles.Domain.Enums;
using Profiles.Infrastructure.Persistence;
using Profiles.Infrastructure.Persistence.Mappers;

namespace Profiles.Infrastructure.Repositories;

public sealed class DoctorRepository : IDoctorRepository
{
    private readonly ApplicationDbContext _context;

    public DoctorRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Doctor doctor, CancellationToken cancellationToken = default)
    {
        await _context.Doctors.AddAsync(doctor.ToEntity(), cancellationToken);
    }

    public Task<int> DeactivateByOfficeAsync(Guid officeId, CancellationToken cancellationToken = default)
    {
        var inactive = DoctorStatus.Inactive.ToString();

        return _context.Doctors
            .Where(doctor => doctor.OfficeId == officeId && doctor.Status != inactive)
            .ExecuteUpdateAsync(setters => setters.SetProperty(doctor => doctor.Status, inactive), cancellationToken);
    }
}
