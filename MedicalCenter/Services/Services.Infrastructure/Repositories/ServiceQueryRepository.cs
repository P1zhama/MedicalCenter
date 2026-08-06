using Microsoft.EntityFrameworkCore;
using Services.Application.Common.Dtos;
using Services.Application.Common.Interfaces;
using Services.Domain.Enums;
using Services.Infrastructure.Persistence;

namespace Services.Infrastructure.Repositories;

public sealed class ServiceQueryRepository : IServiceQueryRepository
{
    private readonly ServicesDbContext _context;

    public ServiceQueryRepository(ServicesDbContext context)
    {
        _context = context;
    }

    public Task<ServiceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => (from service in _context.Services.AsNoTracking()
            join category in _context.ServiceCategories.AsNoTracking()
                on service.CategoryId equals category.Id
            join specialization in _context.Specializations.AsNoTracking()
                on service.SpecializationId equals specialization.Id
            where service.Id == id
            select new ServiceDto(
                service.Id,
                service.Name,
                service.Price,
                service.Status,
                category.Id,
                category.Name,
                specialization.Id,
                specialization.Name))
            .FirstOrDefaultAsync(cancellationToken)!;

    public Task<ServiceForAppointmentDto?> GetForAppointmentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var activeStatus = ActivityStatus.Active.ToString();

        return (from service in _context.Services.AsNoTracking()
                join category in _context.ServiceCategories.AsNoTracking()
                    on service.CategoryId equals category.Id
                where service.Id == id
                select new ServiceForAppointmentDto(
                    service.Id,
                    service.Name,
                    service.Price,
                    service.SpecializationId,
                    service.CategoryId,
                    category.TimeSlotMinutes,
                    service.Status == activeStatus))
            .FirstOrDefaultAsync(cancellationToken)!;
    }

    public async Task<ServiceCatalogDto> GetActiveCatalogAsync(CancellationToken cancellationToken = default)
    {
        var activeStatus = ActivityStatus.Active.ToString();

        var rows = await (
            from service in _context.Services.AsNoTracking()
            join category in _context.ServiceCategories.AsNoTracking()
                on service.CategoryId equals category.Id
            join specialization in _context.Specializations.AsNoTracking()
                on service.SpecializationId equals specialization.Id
            where service.Status == activeStatus && specialization.Status == activeStatus
            select new CatalogRow(
                category.Id,
                category.Name,
                specialization.Id,
                specialization.Name,
                service.Id,
                service.Name,
                service.Price))
            .ToListAsync(cancellationToken);

        var categories = rows
            .GroupBy(row => new { row.CategoryId, row.CategoryName })
            .OrderBy(category => category.Key.CategoryName)
            .Select(category => new CatalogCategoryDto(
                category.Key.CategoryId,
                category.Key.CategoryName,
                category
                    .GroupBy(row => new { row.SpecializationId, row.SpecializationName })
                    .OrderBy(specialization => specialization.Key.SpecializationName)
                    .Select(specialization => new CatalogSpecializationDto(
                        specialization.Key.SpecializationId,
                        specialization.Key.SpecializationName,
                        specialization
                            .OrderBy(row => row.ServiceName)
                            .Select(row => new CatalogServiceDto(row.ServiceId, row.ServiceName, row.Price))
                            .ToList()))
                    .ToList()))
            .ToList();

        return new ServiceCatalogDto(categories);
    }

    private sealed record CatalogRow(
        Guid CategoryId,
        string CategoryName,
        Guid SpecializationId,
        string SpecializationName,
        Guid ServiceId,
        string ServiceName,
        decimal Price);
}
