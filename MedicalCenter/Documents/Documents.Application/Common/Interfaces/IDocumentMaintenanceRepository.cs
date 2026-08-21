using Documents.Application.Common.Dtos;

namespace Documents.Application.Common.Interfaces;

public interface IDocumentMaintenanceRepository
{
    Task<IReadOnlyList<SweepCandidateDto>> GetSweepCandidatesAsync(
        DateTimeOffset createdBefore,
        DateTimeOffset verifiedBefore,
        int limit,
        CancellationToken cancellationToken = default);

    Task MarkVerifiedAsync(
        IReadOnlyCollection<Guid> ids,
        DateTimeOffset verifiedAt,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
}
