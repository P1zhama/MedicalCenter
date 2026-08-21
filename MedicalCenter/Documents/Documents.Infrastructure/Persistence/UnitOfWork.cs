using Documents.Application.Common.Interfaces;

namespace Documents.Infrastructure.Persistence;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly DocumentsDbContext _context;

    public UnitOfWork(DocumentsDbContext context)
    {
        _context = context;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
