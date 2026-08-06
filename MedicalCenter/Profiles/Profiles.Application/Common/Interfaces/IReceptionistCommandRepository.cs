using Profiles.Domain;

namespace Profiles.Application.Common.Interfaces;

public interface IReceptionistCommandRepository
{
    Task AddAsync(Receptionist receptionist, CancellationToken cancellationToken = default);

    Task<Receptionist?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Receptionist>> GetByOfficeAsync(Guid officeId, CancellationToken cancellationToken = default);

    void Update(Receptionist receptionist, long expectedVersion);

    void Remove(Guid id);
}
