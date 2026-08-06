using Profiles.Application.Common.Dtos;

namespace Profiles.Application.Common.Interfaces;

public interface IReceptionistQueryRepository
{
    Task<IReadOnlyList<ReceptionistListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ReceptionistDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ReceptionistDto?> GetByAccountIdAsync(Guid accountId, CancellationToken cancellationToken = default);
}
