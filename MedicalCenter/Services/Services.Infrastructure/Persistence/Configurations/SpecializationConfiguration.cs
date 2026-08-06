using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Services.Infrastructure.Persistence.Entities;

namespace Services.Infrastructure.Persistence.Configurations;

public class SpecializationConfiguration : IEntityTypeConfiguration<SpecializationEntity>
{
    public void Configure(EntityTypeBuilder<SpecializationEntity> builder)
    {
        builder.ToTable("Specializations");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100)
            .UseCollation("Latin1_General_100_CI_AS");

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

        builder.HasIndex(s => s.Name).IsUnique();
    }
}
