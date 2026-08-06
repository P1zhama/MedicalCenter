using Profiles.Domain;

namespace Profiles.Application.Common.Interfaces;

public interface IDoctorCommandRepository
{
    Task AddAsync(Doctor doctor, CancellationToken cancellationToken = default);

    Task<Doctor?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Doctor>> GetByOfficeAsync(Guid officeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Doctor>> GetBySpecializationAsync(Guid specializationId, CancellationToken cancellationToken = default);

    void Update(Doctor doctor, long expectedVersion);

    void Remove(Guid id);
}
