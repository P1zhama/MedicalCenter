using Authorization.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Authorization.Application.Commands.DeleteWorkerAccount;

public class DeleteWorkerAccountCommandHandler : IRequestHandler<DeleteWorkerAccountCommand, bool>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<DeleteWorkerAccountCommandHandler> _logger;

    public DeleteWorkerAccountCommandHandler(
        IAccountRepository accountRepository,
        ILogger<DeleteWorkerAccountCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(DeleteWorkerAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);

        if (account == null)
        {
            _logger.LogWarning("Compensation: account {AccountId} not found, nothing to delete.", request.AccountId);
            return false;
        }

        await _accountRepository.DeleteAsync(account, cancellationToken);

        _logger.LogInformation("Compensation: orphaned worker account {AccountId} deleted.", request.AccountId);

        return true;
    }
}
