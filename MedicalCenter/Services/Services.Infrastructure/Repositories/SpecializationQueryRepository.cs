using Microsoft.EntityFrameworkCore;
using Services.Application.Common.Dtos;
using Services.Application.Common.Interfaces;
using Services.Domain.Enums;
using Services.Infrastructure.Persistence;

namespace Services.Infrastructure.Repositories;

public sealed class SpecializationQueryRepository : ISpecializationQueryRepository
{
    private readonly ServicesDbContext _context;

    public SpecializationQueryRepository(ServicesDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SpecializationListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Specializations
            .AsNoTracking()
            .OrderBy(specialization => specialization.Name)
            .Select(specialization => new SpecializationListItemDto(
                specialization.Id,
                specialization.Name,
                specialization.Status))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<PublicSpecializationDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var activeStatus = ActivityStatus.Active.ToString();

        return await _context.Specializations
            .AsNoTracking()
            .Where(specialization => specialization.Status == activeStatus)
            .OrderBy(specialization => specialization.Name)
            .Select(specialization => new PublicSpecializationDto(
                specialization.Id,
                specialization.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<SpecializationDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specialization = await _context.Specializations
            .AsNoTracking()
            .Where(entity => entity.Id == id)
            .Select(entity => new { entity.Id, entity.Name, entity.Status })
            .FirstOrDefaultAsync(cancellationToken);

        if (specialization is null)
            return null;

        var services = await (
            from service in _context.Services.AsNoTracking()
            join category in _context.ServiceCategories.AsNoTracking()
                on service.CategoryId equals category.Id
            where service.SpecializationId == id
            orderby service.Name
            select new ServiceListItemDto(
                service.Id,
                service.Name,
                service.Price,
                service.Status,
                category.Id,
                category.Name))
            .ToListAsync(cancellationToken);

        return new SpecializationDto(
            specialization.Id,
            specialization.Name,
            specialization.Status,
            services);
    }

    public Task<bool> IsActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var activeStatus = ActivityStatus.Active.ToString();

        return _context.Specializations
            .AsNoTracking()
            .AnyAsync(specialization => specialization.Id == id && specialization.Status == activeStatus, cancellationToken);
    }
}
