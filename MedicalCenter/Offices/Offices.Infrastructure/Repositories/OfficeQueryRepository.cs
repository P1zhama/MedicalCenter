using MongoDB.Driver;
using Offices.Application.Common.Dtos;
using Offices.Application.Common.Interfaces;
using Offices.Domain.Enums;
using Offices.Infrastructure.Persistence;

namespace Offices.Infrastructure.Repositories;

public sealed class OfficeQueryRepository : IOfficeQueryRepository
{
    private readonly IMongoCollection<OfficeDocument> _offices;

    public OfficeQueryRepository(OfficesDbContext context)
    {
        _offices = context.Offices;
    }

    public async Task<OfficeDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var document = await _offices.Find(office => office.Id == id).FirstOrDefaultAsync(cancellationToken);

        return document is null ? null : ToDto(document);
    }

    public async Task<IReadOnlyList<OfficeListItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _offices.Find(FilterDefinition<OfficeDocument>.Empty).ToListAsync(cancellationToken);

        return documents.Select(ToListItem).ToList();
    }

    public async Task<IReadOnlyList<PublicOfficeDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var activeStatus = OfficeStatus.Active.ToString();

        var documents = await _offices
            .Find(office => office.Status == activeStatus)
            .ToListAsync(cancellationToken);

        return documents
            .Select(document => new PublicOfficeDto(
                document.Id,
                FormatAddress(document),
                document.PhotoUrl,
                document.RegistryPhoneNumber))
            .OrderBy(office => office.Address)
            .ToList();
    }

    public Task<bool> IsActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var activeStatus = OfficeStatus.Active.ToString();

        return _offices
            .Find(office => office.Id == id && office.Status == activeStatus)
            .AnyAsync(cancellationToken);
    }

    private static OfficeDto ToDto(OfficeDocument document) => new(
        document.Id,
        document.PhotoUrl,
        FormatAddress(document),
        document.City,
        document.Street,
        document.HouseNumber,
        document.OfficeNumber,
        document.Status,
        document.RegistryPhoneNumber);

    private static OfficeListItemDto ToListItem(OfficeDocument document) => new(
        document.Id,
        FormatAddress(document),
        document.Status,
        document.RegistryPhoneNumber);

    private static string FormatAddress(OfficeDocument document)
    {
        var parts = new List<string> { document.City, document.Street, document.HouseNumber };

        if (!string.IsNullOrWhiteSpace(document.OfficeNumber))
            parts.Add(document.OfficeNumber);

        return string.Join(", ", parts);
    }
}
