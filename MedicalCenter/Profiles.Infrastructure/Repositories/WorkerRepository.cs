

using Microsoft.EntityFrameworkCore;
using Profiles.Application.Common.Interfaces;
using Profiles.Domain;
using Profiles.Domain.Enums;
using Profiles.Infrastructure.Persistence;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Profiles.Infrastructure.Repositories;

public class WorkerRepository : IWorkerRepository
{
    private readonly ApplicationDbContext _context;

    public WorkerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddDoctorAsync(Doctor doctor, CancellationToken cancellationToken)
    {
        await _context.Doctors.AddAsync(doctor, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddReceptionistAsync(Receptionist receptionist, CancellationToken cancellationToken)
    {
        await _context.Receptionists.AddAsync(receptionist, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> DeactivateDoctorsByOfficeAsync(Guid officeId, CancellationToken cancellationToken)
    {
        return await _context.Doctors
            .Where(d => d.OfficeId == officeId && d.Status != DoctorStatus.Inactive)
            .ExecuteUpdateAsync(
                s => s.SetProperty(d => d.Status, DoctorStatus.Inactive),
                cancellationToken);
    }
}