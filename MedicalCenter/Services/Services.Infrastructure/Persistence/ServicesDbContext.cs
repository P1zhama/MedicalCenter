using MassTransit;
using Microsoft.EntityFrameworkCore;
using Services.Infrastructure.Persistence.Entities;

namespace Services.Infrastructure.Persistence;

public class ServicesDbContext : DbContext
{
    public ServicesDbContext(DbContextOptions<ServicesDbContext> options) : base(options)
    {
    }

    public DbSet<SpecializationEntity> Specializations => Set<SpecializationEntity>();

    public DbSet<ServiceEntity> Services => Set<ServiceEntity>();

    public DbSet<ServiceCategoryEntity> ServiceCategories => Set<ServiceCategoryEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ServicesDbContext).Assembly);
    }
}
