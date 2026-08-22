using MongoDB.Driver;
using Offices.Application.Common.Interfaces;
using Offices.Infrastructure.Persistence;

namespace Offices.Infrastructure.Repositories;

public sealed class PhotoReferenceRepository : IPhotoReferenceRepository
{
    private readonly IMongoCollection<OfficeDocument> _offices;

    public PhotoReferenceRepository(OfficesDbContext context)
    {
        _offices = context.Offices;
    }

    public async Task<IReadOnlyList<string>> GetPhotoUrlsAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _offices
            .Find(office => office.PhotoUrl != null)
            .Project(office => office.PhotoUrl!)
            .ToListAsync(cancellationToken);

        return documents.Distinct().ToList();
    }
}
