using Offices.Domain;

namespace Offices.Application.Common.Interfaces;

public interface IOfficeRepository
{
    Task AddAsync(Office office, CancellationToken cancellationToken);
    Task<Office?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Office>> GetAllAsync(CancellationToken cancellationToken);
    Task UpdateAsync(Office office, CancellationToken cancellationToken);
}
