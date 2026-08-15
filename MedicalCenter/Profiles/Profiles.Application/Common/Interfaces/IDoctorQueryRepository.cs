using Profiles.Application.Common.Dtos;

namespace Profiles.Application.Common.Interfaces;

public interface IDoctorQueryRepository
{
    Task<IReadOnlyList<DoctorCardDto>> GetActiveCardsAsync(
        DoctorFilter filter,
        int currentYear,
        CancellationToken cancellationToken = default);

    Task<DoctorCardDto?> GetActiveCardByIdAsync(
        Guid id,
        int currentYear,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DoctorListItemDto>> SearchAsync(
        DoctorFilter filter,
        CancellationToken cancellationToken = default);

    Task<DoctorDto?> GetByIdAsync(Guid id, int currentYear, CancellationToken cancellationToken = default);

    Task<DoctorDto?> GetByAccountIdAsync(Guid accountId, int currentYear, CancellationToken cancellationToken = default);

    Task<DoctorForAppointmentDto?> GetForAppointmentAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DoctorSummaryDto>> GetSummariesAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);
}
