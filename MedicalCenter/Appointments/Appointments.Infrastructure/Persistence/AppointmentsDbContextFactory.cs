using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Appointments.Infrastructure.Persistence;

public sealed class AppointmentsDbContextFactory : IDesignTimeDbContextFactory<AppointmentsDbContext>
{
    private const string DesignTimeConnectionString =
        "Host=localhost;Port=5432;Database=MmcAppointmentsDb;Username=postgres;Password=postgres";

    public AppointmentsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppointmentsDbContext>();
        optionsBuilder.UseNpgsql(DesignTimeConnectionString);

        return new AppointmentsDbContext(optionsBuilder.Options);
    }
}
