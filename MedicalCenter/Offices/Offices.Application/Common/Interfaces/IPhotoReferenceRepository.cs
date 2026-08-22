namespace Offices.Application.Common.Interfaces;

public interface IPhotoReferenceRepository
{
    Task<IReadOnlyList<string>> GetPhotoUrlsAsync(CancellationToken cancellationToken = default);
}
