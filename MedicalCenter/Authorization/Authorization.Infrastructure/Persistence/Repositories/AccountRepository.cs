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

    public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return _context.Accounts.AnyAsync(a => a.Email == email.Value, cancellationToken);
    }
}
