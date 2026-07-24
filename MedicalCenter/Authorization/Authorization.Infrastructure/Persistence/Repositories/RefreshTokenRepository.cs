using Authorization.Application.Common.Interfaces;
using Authorization.Domain;
using Authorization.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Infrastructure.Persistence.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AuthDbContext _context;

    public RefreshTokenRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken.ToEntity(), cancellationToken);
    }

    public async Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        var entity = await _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        return entity?.ToDomain();
    }

    public Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _context.RefreshTokens.Update(refreshToken.ToEntity());

        return Task.CompletedTask;
    }

    public Task RevokeAllActiveForAccountAsync(Guid accountId, DateTimeOffset revokedAt, CancellationToken cancellationToken = default)
    {
        return _context.RefreshTokens
            .Where(token => token.AccountId == accountId && token.RevokedAt == null && token.ExpiresAt > revokedAt)
            .ExecuteUpdateAsync(setters => setters.SetProperty(token => token.RevokedAt, revokedAt), cancellationToken);
    }
}
