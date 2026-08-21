using Documents.Application.Common.Interfaces;
using Documents.Domain;
using Documents.Infrastructure.Persistence;
using Documents.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Documents.Infrastructure.Repositories;

public sealed class DocumentCommandRepository : IDocumentCommandRepository
{
    private readonly DocumentsDbContext _context;

    public DocumentCommandRepository(DocumentsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(StoredDocument document, CancellationToken cancellationToken = default)
    {
        await _context.Documents.AddAsync(document.ToEntity(), cancellationToken);
    }

    public async Task<StoredDocument?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Documents
            .AsNoTracking()
            .FirstOrDefaultAsync(document => document.Id == id, cancellationToken);

        return entity?.ToDomain();
    }

    public void Remove(StoredDocument document)
    {
        var entry = _context.Documents.Attach(document.ToEntity());

        entry.State = EntityState.Deleted;
    }
}
