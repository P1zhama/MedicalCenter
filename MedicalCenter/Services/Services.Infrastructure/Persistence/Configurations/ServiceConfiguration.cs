using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Services.Infrastructure.Persistence.Entities;

namespace Services.Infrastructure.Persistence.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<ServiceEntity>
{
    public void Configure(EntityTypeBuilder<ServiceEntity> builder)
    {
        builder.ToTable("Services");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200)
            .UseCollation("Latin1_General_100_CI_AS");

        builder.Property(s => s.Price)
            .HasColumnName("price")
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(s => s.SpecializationId)
            .HasColumnName("specialization_id")
            .IsRequired();

        builder.Property(s => s.CategoryId)
            .HasColumnName("category_id")
            .IsRequired();

        builder.Property(s => s.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(s => s.Version)
            .HasColumnName("version")
            .IsRequired()
            .IsConcurrencyToken();

        builder.Property(s => s.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.UpdatedBy)
            .HasColumnName("updated_by");

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(s => new { s.SpecializationId, s.Name }).IsUnique();
        builder.HasIndex(s => s.CategoryId);
        builder.HasIndex(s => s.Status);

        builder.HasOne<SpecializationEntity>()
            .WithMany()
            .HasForeignKey(s => s.SpecializationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ServiceCategoryEntity>()
            .WithMany()
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
