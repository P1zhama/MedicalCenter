using Common.Abstractions.Paging;
using Profiles.Application.Common.Dtos;

namespace Profiles.Application.Common.Interfaces;

public interface IDoctorQueryRepository
{
    Task<PagedResult<DoctorCardDto>> GetActiveCardsAsync(
        DoctorFilter filter,
        int currentYear,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<DoctorCardDto?> GetActiveCardByIdAsync(
        Guid id,
        int currentYear,
        CancellationToken cancellationToken = default);

    Task<PagedResult<DoctorListItemDto>> SearchAsync(
        DoctorFilter filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<DoctorDto?> GetByIdAsync(Guid id, int currentYear, CancellationToken cancellationToken = default);

    Task<DoctorDto?> GetByAccountIdAsync(Guid accountId, int currentYear, CancellationToken cancellationToken = default);

    Task<DoctorForAppointmentDto?> GetForAppointmentAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetAtWorkIdsAsync(
        Guid specializationId,
        Guid? officeId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DoctorSummaryDto>> GetSummariesAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);
}
