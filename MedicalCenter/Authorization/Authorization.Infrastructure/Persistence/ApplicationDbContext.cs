using Authorization.Domain;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<AccountRole> AccountRoles => Set<AccountRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("Accounts");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(254);
            entity.Property(e => e.Password).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.IsProfileCreated).HasDefaultValue(false);

            entity.Property(e => e.RefreshToken).HasMaxLength(255);

            entity.Property(e => e.CreatedBy).HasMaxLength(254);
            entity.Property(e => e.UpdatedBy).HasMaxLength(254);

            entity.HasOne(e => e.Photo)
                .WithOne(p => p.Account)
                .HasForeignKey<Photo>(p => p.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Photo>(entity =>
        {
            entity.ToTable("Photos");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Url).IsRequired();
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);

            entity.HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "Patient" },
                new Role { Id = 3, Name = "Doctor" },
                new Role { Id = 4, Name = "Receptionist" }
            );
        });

        modelBuilder.Entity<AccountRole>(entity =>
        {
            entity.ToTable("AccountRoles");
            entity.HasKey(e => new { e.AccountId, e.RoleId });

            entity.HasOne(e => e.Account)
                .WithMany(a => a.AccountRoles)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.AccountRoles)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}