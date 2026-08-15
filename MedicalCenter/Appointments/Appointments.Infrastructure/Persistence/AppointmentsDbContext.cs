using System.Reflection;
using Appointments.Infrastructure.Persistence.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Appointments.Infrastructure.Persistence;

public class AppointmentsDbContext : DbContext
{
    public AppointmentsDbContext(DbContextOptions<AppointmentsDbContext> options) : base(options)
    {
    }

    public DbSet<AppointmentEntity> Appointments => Set<AppointmentEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        base.OnModelCreating(modelBuilder);
    }
}
