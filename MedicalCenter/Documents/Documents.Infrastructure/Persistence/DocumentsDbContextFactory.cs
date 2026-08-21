using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Documents.Infrastructure.Persistence;

public sealed class DocumentsDbContextFactory : IDesignTimeDbContextFactory<DocumentsDbContext>
{
    private const string DesignTimeConnection =
        "Host=localhost;Port=5432;Database=MmcDocumentsDb;Username=postgres;Password=postgres";

    public DocumentsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? DesignTimeConnection;

        var options = new DbContextOptionsBuilder<DocumentsDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new DocumentsDbContext(options);
    }
}
