using Authorization.Application.Common.Interfaces;
using Authorization.Domain;
using Authorization.Domain.ValueObjects;
using Authorization.Infrastructure.Persistence;
using Authorization.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Infrastructure.Repositories;

public sealed class AccountRepository : IAccountRepository
{
    private readonly AuthDbContext _context;

    public AccountRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
    {
        await _context.Accounts.AddAsync(account.ToEntity(), cancellationToken);
    }

    public async Task<Account?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Accounts
            .Include(a => a.Claims)
            .FirstOrDefaultAsync(a => a.Email == email.Value, cancellationToken);

        return entity?.ToDomain();
    }

    public async Task<Account?> GetByEmailConfirmationTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Accounts
            .Include(a => a.Claims)
            .FirstOrDefaultAsync(a => a.EmailConfirmationTokenHash == tokenHash, cancellationToken);

        return entity?.ToDomain();
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Accounts
            .AsNoTracking()
            .Include(a => a.Claims)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        return entity?.ToDomain();
    }

    public async Task UpdateAsync(Account account, long expectedVersion, CancellationToken cancellationToken = default)
    {
        var tracked = await _context.Accounts
            .Include(a => a.Claims)
            .FirstOrDefaultAsync(a => a.Id == account.Id, cancellationToken);

        if (tracked is null)
            throw new InvalidOperationException($"Account {account.Id} must be loaded before update.");

        var entity = account.ToEntity();
        var entry = _context.Entry(tracked);

        entry.CurrentValues.SetValues(entity);
        entry.Property(a => a.Version).OriginalValue = expectedVersion;

        foreach (var claim in entity.Claims.Where(claim => tracked.Claims.All(existing => existing.Id != claim.Id)))
        {
            tracked.Claims.Add(claim);
        }

        foreach (var removed in tracked.Claims.Where(existing => entity.Claims.All(claim => claim.Id != existing.Id)).ToList())
        {
            tracked.Claims.Remove(removed);
        }
    }

    public Task<int> DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.Accounts
            .Where(account => account.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return _context.Accounts.AnyAsync(a => a.Email == email.Value, cancellationToken);
    }
}
