using Authorization.Domain;
using Authorization.Infrastructure.Persistence.Entities;

namespace Authorization.Infrastructure.Persistence.Mappers;

public static class RefreshTokenMapper
{
    public static RefreshTokenEntity ToEntity(this RefreshToken refreshToken) => new()
    {
        Id = refreshToken.Id,
        AccountId = refreshToken.AccountId,
        TokenHash = refreshToken.TokenHash,
        CreatedAt = refreshToken.CreatedAt,
        ExpiresAt = refreshToken.ExpiresAt,
        RevokedAt = refreshToken.RevokedAt,
        ReplacedByTokenId = refreshToken.ReplacedByTokenId
    };

    public static RefreshToken ToDomain(this RefreshTokenEntity entity) => RefreshToken.Restore(
        entity.Id,
        entity.AccountId,
        entity.TokenHash,
        entity.CreatedAt,
        entity.ExpiresAt,
        entity.RevokedAt,
        entity.ReplacedByTokenId);
}
