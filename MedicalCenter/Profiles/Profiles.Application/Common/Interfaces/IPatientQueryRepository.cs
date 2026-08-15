using Profiles.Application.Common.Dtos;

namespace Profiles.Application.Common.Interfaces;

public interface IPatientQueryRepository
{
    Task<IReadOnlyList<PatientListItemDto>> SearchAsync(
        string? fullNameSearch,
        CancellationToken cancellationToken = default);

    Task<PatientDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PatientDto?> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PatientSummaryDto>> GetSummariesAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);
}
