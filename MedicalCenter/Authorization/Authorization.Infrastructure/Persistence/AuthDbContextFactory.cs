using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Authorization.Infrastructure.Persistence;

public sealed class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    private const string DesignTimeConnectionString =
        "Server=localhost;Database=MmcAuthDb;Trusted_Connection=True;TrustServerCertificate=True";

    public AuthDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
        optionsBuilder.UseSqlServer(DesignTimeConnectionString);

        return new AuthDbContext(optionsBuilder.Options);
    }
}
