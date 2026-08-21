using Documents.Domain;

namespace Documents.Application.Common.Interfaces;

public interface IDocumentQueryRepository
{
    Task<StoredDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
