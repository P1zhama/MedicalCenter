using Profiles.Application.Common.Interfaces;
using Profiles.Domain;
using Profiles.Infrastructure.Persistence;

namespace Profiles.Infrastructure.Repositories;

public class WorkerRepository : IWorkerRepository
{
    private readonly ApplicationDbContext _context;

    public WorkerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddReceptionistAsync(Receptionist receptionist, CancellationToken cancellationToken)
    {
        await _context.Receptionists.AddAsync(receptionist, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
