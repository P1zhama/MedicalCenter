namespace Documents.Application.Common.Interfaces;

public interface IPhotoReferenceClient
{
    Task<IReadOnlyList<Guid>> GetReferencedAsync(
        IReadOnlyCollection<Guid> documentIds,
        CancellationToken cancellationToken = default);
}
