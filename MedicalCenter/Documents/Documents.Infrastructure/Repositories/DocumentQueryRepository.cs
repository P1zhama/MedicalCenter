using Documents.Application.Common.Interfaces;
using Documents.Domain;
using Documents.Infrastructure.Persistence;
using Documents.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Documents.Infrastructure.Repositories;

public sealed class DocumentQueryRepository : IDocumentQueryRepository
{
    private readonly DocumentsDbContext _context;

    public DocumentQueryRepository(DocumentsDbContext context)
    {
        _context = context;
    }

    public async Task<StoredDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(document => document.Id == id, cancellationToken);

        return entity?.ToDomain();
    }
}
