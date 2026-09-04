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

    public void Remove(StoredDocument document)
    {
        var entry = _context.Documents.Attach(document.ToEntity());

        entry.State = EntityState.Deleted;
    }
}
