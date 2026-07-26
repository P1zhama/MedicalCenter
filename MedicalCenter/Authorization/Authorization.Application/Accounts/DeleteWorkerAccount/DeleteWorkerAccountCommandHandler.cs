using Authorization.Application.Common.Interfaces;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Authorization.Application.Accounts.DeleteWorkerAccount;

public sealed class DeleteWorkerAccountCommandHandler
    : IRequestHandler<DeleteWorkerAccountCommand, ErrorOr<Success>>
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

    public async Task<ErrorOr<Success>> Handle(DeleteWorkerAccountCommand request, CancellationToken cancellationToken)
    {
        var deleted = await _accountRepository.DeleteByIdAsync(request.AccountId, cancellationToken);

        _logger.LogInformation(
            "Delete worker account {AccountId} affected {RowCount} row(s)",
            request.AccountId,
            deleted);

        return Result.Success;
    }
}
