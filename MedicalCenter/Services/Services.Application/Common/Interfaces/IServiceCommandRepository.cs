using Services.Domain.Models;

namespace Services.Application.Common.Interfaces;

public interface IServiceCommandRepository
{
    Task AddAsync(Service service, CancellationToken cancellationToken = default);

    Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Service>> GetBySpecializationAsync(Guid specializationId, CancellationToken cancellationToken = default);

    Task<bool> ExistsWithNameAsync(
        string name,
        Guid specializationId,
        Guid? excludingId = null,
        CancellationToken cancellationToken = default);

    void Update(Service service, long expectedVersion);
}
