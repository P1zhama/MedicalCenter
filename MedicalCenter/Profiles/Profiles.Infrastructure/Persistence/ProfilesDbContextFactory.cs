using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Profiles.Infrastructure.Persistence;

public sealed class ProfilesDbContextFactory : IDesignTimeDbContextFactory<ProfilesDbContext>
{
    private const string DesignTimeConnectionString =
        "Server=localhost;Database=MmcProfilesDb;Trusted_Connection=True;TrustServerCertificate=True";

    public ProfilesDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ProfilesDbContext>();
        optionsBuilder.UseSqlServer(DesignTimeConnectionString);

        return new ProfilesDbContext(optionsBuilder.Options);
    }
}
