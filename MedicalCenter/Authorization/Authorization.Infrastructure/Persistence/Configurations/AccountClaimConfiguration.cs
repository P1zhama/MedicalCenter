using Authorization.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization.Infrastructure.Persistence.Configurations;

public class AccountClaimConfiguration : IEntityTypeConfiguration<AccountClaimEntity>
{
    public void Configure(EntityTypeBuilder<AccountClaimEntity> builder)
    {
        builder.ToTable("AccountClaims");
        builder.HasKey(claim => claim.Id);
        builder.Property(claim => claim.Id).HasColumnName("id");

        builder.Property(claim => claim.AccountId)
            .HasColumnName("account_id")
            .IsRequired();

        builder.Property(claim => claim.Type)
            .HasColumnName("type")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(claim => claim.Value)
            .HasColumnName("value")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(claim => claim.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasOne(claim => claim.Account)
            .WithMany(account => account.Claims)
            .HasForeignKey(claim => claim.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(claim => new { claim.AccountId, claim.Type, claim.Value }).IsUnique();
    }
}
