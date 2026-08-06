using Authorization.Application.Common.Interfaces;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Authorization.Application.Accounts.DeletePatientAccount;

public sealed class DeletePatientAccountCommandHandler
    : IRequestHandler<DeletePatientAccountCommand, ErrorOr<Success>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<DeletePatientAccountCommandHandler> _logger;

    public DeletePatientAccountCommandHandler(
        IAccountRepository accountRepository,
        ILogger<DeletePatientAccountCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<ErrorOr<Success>> Handle(
        DeletePatientAccountCommand request,
        CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);

        if (account is null)
        {
            _logger.LogInformation(
                "Delete patient account {AccountId} skipped: account was not found",
                request.AccountId);

            return Result.Success;
        }

        if (account.IsWorker)
        {
            _logger.LogWarning(
                "Refused to delete account {AccountId}: role {Role} is a worker role",
                request.AccountId,
                account.Role);

            return Error.Forbidden("Account.NotAPatient", "Only patient accounts can be deleted through this operation.");
        }

        var deleted = await _accountRepository.DeleteByIdAsync(request.AccountId, cancellationToken);

        _logger.LogInformation(
            "Delete patient account {AccountId} affected {RowCount} row(s)",
            request.AccountId,
            deleted);

        return Result.Success;
    }
}
