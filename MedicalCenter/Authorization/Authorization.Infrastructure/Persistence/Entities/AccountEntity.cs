namespace Authorization.Infrastructure.Persistence.Entities;

public class AccountEntity
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTimeOffset? EmailConfirmedAt { get; set; }

    public string? EmailConfirmationTokenHash { get; set; }

    public DateTimeOffset? EmailConfirmationTokenExpiresAt { get; set; }

    public long Version { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public List<AccountClaimEntity> Claims { get; set; } = [];
}
