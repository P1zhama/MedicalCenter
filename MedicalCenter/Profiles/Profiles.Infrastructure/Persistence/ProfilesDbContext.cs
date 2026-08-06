using MassTransit;
using Microsoft.EntityFrameworkCore;
using Profiles.Infrastructure.Persistence.Entities;
using System.Reflection;

namespace Profiles.Infrastructure.Persistence;

public class ProfilesDbContext : DbContext
{
    public ProfilesDbContext(DbContextOptions<ProfilesDbContext> options)
        : base(options)
    {
    }

    public DbSet<PatientEntity> Patients => Set<PatientEntity>();

    public DbSet<DoctorEntity> Doctors => Set<DoctorEntity>();

    public DbSet<ReceptionistEntity> Receptionists => Set<ReceptionistEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
