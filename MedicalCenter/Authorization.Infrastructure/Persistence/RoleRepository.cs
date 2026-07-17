using Authorization.Application.Common.Interfaces;
using Authorization.Domain;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.Persistence;

public class RoleRepository : IRoleRepository
{
    private readonly ApplicationDbContext _context;

    public RoleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);
    }

    public async Task<Role?> GetByIdAsync(int roleId, CancellationToken cancellationToken)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);
    }
}
