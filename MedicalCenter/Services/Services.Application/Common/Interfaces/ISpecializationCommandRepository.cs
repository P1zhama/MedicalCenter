using Services.Domain.Models;

namespace Services.Application.Common.Interfaces;

public interface ISpecializationCommandRepository
{
    Task AddAsync(Specialization specialization, CancellationToken cancellationToken = default);

    Task<Specialization?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsWithNameAsync(string name, Guid? excludingId = null, CancellationToken cancellationToken = default);

    void Update(Specialization specialization, long expectedVersion);
}
