using Documents.Domain;

namespace Documents.Application.Common.Interfaces;

public interface IDocumentCommandRepository
{
    Task AddAsync(StoredDocument document, CancellationToken cancellationToken = default);

    Task<StoredDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Remove(StoredDocument document);
}
