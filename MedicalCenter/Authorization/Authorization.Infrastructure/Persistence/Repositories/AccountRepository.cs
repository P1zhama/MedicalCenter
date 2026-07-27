using Authorization.Application.Common.Interfaces;
using Authorization.Domain;
using Authorization.Domain.ValueObjects;
using Authorization.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Infrastructure.Persistence.Repositories;

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

    public async Task UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
        var tracked = await _context.Accounts.FindAsync([account.Id], cancellationToken);

        if (tracked is null)
            throw new InvalidOperationException($"Account {account.Id} must be loaded before update.");

        _context.Entry(tracked).CurrentValues.SetValues(account.ToEntity());
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
