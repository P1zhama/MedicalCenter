using Authorization.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;


namespace Authorization.Application.Common.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Account?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<bool> IsEmailUniqueAsync(string email, CancellationToken cancellationToken);
    Task AddAsync(Account account, CancellationToken cancellationToken);
    Task UpdateAsync(Account account, CancellationToken cancellationToken);
    Task DeleteAsync(Account account, CancellationToken cancellationToken);
    Task<Account?> GetByRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
}