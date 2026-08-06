using Authorization.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<AccountEntity>
{
    public void Configure(EntityTypeBuilder<AccountEntity> builder)
    {
        builder.ToTable("Accounts");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasColumnName("id");

        builder.Property(a => a.Email)
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(254)
            .UseCollation("Latin1_General_100_CI_AS");

        builder.Property(a => a.PasswordHash)
            .HasColumnName("password_hash")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(a => a.EmailConfirmedAt)
            .HasColumnName("email_confirmed_at");

        builder.Property(a => a.EmailConfirmationTokenHash)
            .HasColumnName("email_confirmation_token_hash")
            .HasMaxLength(200);

        builder.Property(a => a.EmailConfirmationTokenExpiresAt)
            .HasColumnName("email_confirmation_token_expires_at");

        builder.Property(a => a.ProfileId)
            .HasColumnName("profile_id");

        builder.Property(a => a.Version)
            .HasColumnName("version")
            .IsRequired()
            .IsConcurrencyToken();

        builder.Property(a => a.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.UpdatedBy)
            .HasColumnName("updated_by");

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(a => a.Email).IsUnique();
        builder.HasIndex(a => a.EmailConfirmationTokenHash);
    }
}
