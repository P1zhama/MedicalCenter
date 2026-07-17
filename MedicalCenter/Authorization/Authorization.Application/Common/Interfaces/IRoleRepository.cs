
using Authorization.Domain;
using System.Threading;
using System.Threading.Tasks;

namespace Authorization.Application.Common.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken);
    Task<Role?> GetByIdAsync(int roleId, CancellationToken cancellationToken);
}