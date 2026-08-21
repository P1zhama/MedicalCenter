namespace Documents.Application.Common.Interfaces;

public interface IFileStorage
{
    Task SaveAsync(
        string objectKey,
        Stream content,
        string contentType,
        long size,
        CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(string objectKey, CancellationToken cancellationToken = default);

    Task DeleteAsync(string objectKey, CancellationToken cancellationToken = default);
}
