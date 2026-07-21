using Common.Domain;
using ErrorOr;

namespace Authorization.Domain;

public sealed class RefreshToken : Entity<Guid>
{
    public const int TokenHashMaxLength = 200;

    public Guid AccountId { get; }

    public string TokenHash { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset ExpiresAt { get; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public Guid? ReplacedByTokenId { get; private set; }

    public bool IsRevoked => RevokedAt.HasValue;

    private RefreshToken(
        Guid id,
        Guid accountId,
        string tokenHash,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt,
        DateTimeOffset? revokedAt,
        Guid? replacedByTokenId)
        : base(id)
    {
        AccountId = accountId;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        RevokedAt = revokedAt;
        ReplacedByTokenId = replacedByTokenId;
    }

    public static ErrorOr<RefreshToken> Issue(
        Guid id,
        Guid accountId,
        string tokenHash,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt)
    {
        if (id == Guid.Empty)
            return Error.Validation("RefreshToken.Id", "Id must not be empty.");

        if (accountId == Guid.Empty)
            return Error.Validation("RefreshToken.AccountId", "Account id must not be empty.");

        if (string.IsNullOrWhiteSpace(tokenHash))
            return Error.Validation("RefreshToken.TokenHash", "Token hash must not be empty.");

        if (expiresAt <= createdAt)
            return Error.Validation("RefreshToken.ExpiresAt", "Expiration must be later than creation time.");

        return new RefreshToken(id, accountId, tokenHash, createdAt, expiresAt, revokedAt: null, replacedByTokenId: null);
    }

    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;

    public bool IsActive(DateTimeOffset now) => !IsRevoked && !IsExpired(now);

    public ErrorOr<Success> Revoke(DateTimeOffset revokedAt, Guid? replacedByTokenId = null)
    {
        if (IsRevoked)
            return Error.Conflict("RefreshToken.AlreadyRevoked", "Refresh token is already revoked.");

        RevokedAt = revokedAt;
        ReplacedByTokenId = replacedByTokenId;

        return Result.Success;
    }

    public static RefreshToken Restore(
        Guid id,
        Guid accountId,
        string tokenHash,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt,
        DateTimeOffset? revokedAt,
        Guid? replacedByTokenId)
        => new(id, accountId, tokenHash, createdAt, expiresAt, revokedAt, replacedByTokenId);
}
