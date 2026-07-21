namespace Authorization.Infrastructure.Persistence.Entities;

public class RefreshTokenEntity
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    public string TokenHash { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }

    public Guid? ReplacedByTokenId { get; set; }
}
