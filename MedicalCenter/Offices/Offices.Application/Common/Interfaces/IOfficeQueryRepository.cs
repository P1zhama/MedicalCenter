using Offices.Application.Common.Dtos;

namespace Offices.Application.Common.Interfaces;

public interface IOfficeQueryRepository
{
    Task<OfficeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OfficeListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
