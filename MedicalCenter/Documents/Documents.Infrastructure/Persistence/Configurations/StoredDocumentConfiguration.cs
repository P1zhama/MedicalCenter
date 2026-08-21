using Documents.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Documents.Infrastructure.Persistence.Configurations;

public class StoredDocumentConfiguration : IEntityTypeConfiguration<StoredDocumentEntity>
{
    public void Configure(EntityTypeBuilder<StoredDocumentEntity> builder)
    {
        builder.ToTable("documents");
        builder.HasKey(document => document.Id);
        builder.Property(document => document.Id).HasColumnName("id");

        builder.Property(document => document.ObjectKey)
            .HasColumnName("object_key")
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(document => document.FileName)
            .HasColumnName("file_name")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(document => document.ContentType)
            .HasColumnName("content_type")
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(document => document.Size)
            .HasColumnName("size")
            .IsRequired();

        builder.Property(document => document.Kind)
            .HasColumnName("kind")
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(document => document.Visibility)
            .HasColumnName("visibility")
            .IsRequired()
            .HasMaxLength(16);

        builder.Property(document => document.OwnerProfileId)
            .HasColumnName("owner_profile_id");

        builder.Property(document => document.LastVerifiedAt)
            .HasColumnName("last_verified_at");

        builder.Property(document => document.Version)
            .HasColumnName("version")
            .IsRequired()
            .IsConcurrencyToken();

        builder.Property(document => document.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        builder.Property(document => document.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(document => document.UpdatedBy)
            .HasColumnName("updated_by");

        builder.Property(document => document.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(document => document.ObjectKey).IsUnique();
        builder.HasIndex(document => document.OwnerProfileId);
        builder.HasIndex(document => new { document.LastVerifiedAt, document.CreatedAt });
    }
}
