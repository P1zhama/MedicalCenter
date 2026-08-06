using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Services.Infrastructure.Persistence;

public sealed class ServicesDbContextFactory : IDesignTimeDbContextFactory<ServicesDbContext>
{
    private const string DesignTimeConnectionString =
        "Server=localhost;Database=MmcServicesDb;Trusted_Connection=True;TrustServerCertificate=True";

    public ServicesDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ServicesDbContext>();
        optionsBuilder.UseSqlServer(DesignTimeConnectionString);

        return new ServicesDbContext(optionsBuilder.Options);
    }
}
