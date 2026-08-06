using Microsoft.EntityFrameworkCore;
using Services.Application.Common.Dtos;
using Services.Application.Common.Interfaces;
using Services.Infrastructure.Persistence;

namespace Services.Infrastructure.Repositories;

public sealed class ServiceCategoryRepository : IServiceCategoryRepository
{
    private readonly ServicesDbContext _context;

    public ServiceCategoryRepository(ServicesDbContext context)
    {
        _context = context;
    }

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.ServiceCategories
            .AsNoTracking()
            .AnyAsync(category => category.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ServiceCategoryDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.ServiceCategories
            .AsNoTracking()
            .OrderBy(category => category.Name)
            .Select(category => new ServiceCategoryDto(category.Id, category.Name, category.TimeSlotMinutes))
            .ToListAsync(cancellationToken);
}
