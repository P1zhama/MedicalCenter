using Services.Application.Common.Dtos;

namespace Services.Application.Common.Interfaces;

public interface IServiceCategoryRepository
{
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ServiceCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
