using Authorization.Domain;
using Authorization.Domain.ValueObjects;

namespace Authorization.Application.Common.Interfaces;

public interface IAccountRepository
{
    Task AddAsync(Account account, CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default);
}
