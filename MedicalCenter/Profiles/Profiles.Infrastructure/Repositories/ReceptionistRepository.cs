using Profiles.Application.Common.Interfaces;
using Profiles.Domain;
using Profiles.Infrastructure.Persistence;
using Profiles.Infrastructure.Persistence.Mappers;

namespace Profiles.Infrastructure.Repositories;

public sealed class ReceptionistRepository : IReceptionistRepository
{
    private readonly ApplicationDbContext _context;

    public ReceptionistRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Receptionist receptionist, CancellationToken cancellationToken = default)
    {
        await _context.Receptionists.AddAsync(receptionist.ToEntity(), cancellationToken);
    }
}
