using Profiles.Domain;

namespace Profiles.Application.Common.Interfaces;

public interface IDoctorRepository
{
    Task AddAsync(Doctor doctor, CancellationToken cancellationToken = default);

    Task<int> DeactivateByOfficeAsync(Guid officeId, CancellationToken cancellationToken = default);
}
