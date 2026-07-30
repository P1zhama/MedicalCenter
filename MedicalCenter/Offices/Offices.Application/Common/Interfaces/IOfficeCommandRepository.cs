using Offices.Domain.Models;

namespace Offices.Application.Common.Interfaces;

public interface IOfficeCommandRepository
{
    Task AddAsync(Office office, CancellationToken cancellationToken = default);

    Task<Office?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        Office office,
        long expectedVersion,
        IReadOnlyCollection<object> integrationEvents,
        CancellationToken cancellationToken = default);
}
