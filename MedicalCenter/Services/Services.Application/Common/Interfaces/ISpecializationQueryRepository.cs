using Services.Application.Common.Dtos;

namespace Services.Application.Common.Interfaces;

public interface ISpecializationQueryRepository
{
    Task<IReadOnlyList<SpecializationListItemDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<SpecializationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> IsActiveAsync(Guid id, CancellationToken cancellationToken = default);
}
