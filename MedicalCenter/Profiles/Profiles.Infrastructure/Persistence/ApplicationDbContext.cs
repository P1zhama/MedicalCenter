using Microsoft.EntityFrameworkCore;
using Profiles.Domain;
using Profiles.Infrastructure.Persistence.Entities;
using System.Reflection;

namespace Profiles.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<PatientEntity> Patients => Set<PatientEntity>();

    public DbSet<DoctorEntity> Doctors => Set<DoctorEntity>();

    public DbSet<ReceptionistEntity> Receptionists => Set<ReceptionistEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }
}
