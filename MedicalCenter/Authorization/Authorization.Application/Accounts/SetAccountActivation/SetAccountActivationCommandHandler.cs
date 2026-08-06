using Authorization.Application.Common.Interfaces;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Authorization.Application.Accounts.SetAccountActivation;

public sealed class SetAccountActivationCommandHandler
    : IRequestHandler<SetAccountActivationCommand, ErrorOr<Success>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<SetAccountActivationCommandHandler> _logger;

    public SetAccountActivationCommandHandler(
        IAccountRepository accountRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        ILogger<SetAccountActivationCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<ErrorOr<Success>> Handle(SetAccountActivationCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account is null)
        {
            _logger.LogWarning("Activation change skipped: account {AccountId} was not found", request.AccountId);

            return Result.Success;
        }

        var now = _timeProvider.GetUtcNow();
        var expectedVersion = account.Version;

        var result = request.IsActive
            ? account.Reactivate(account.Id, now)
            : account.Deactivate(account.Id, now);

        if (result.IsError)
            return result.Errors;

        if (account.Version == expectedVersion)
            return Result.Success;

        await _accountRepository.UpdateAsync(account, expectedVersion, cancellationToken);

        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
            return Error.Conflict("Account.ConcurrencyConflict", "Account was modified by another operation. Please retry.");

        _logger.LogInformation(
            "Account {AccountId} activation set to {IsActive}",
            request.AccountId,
            request.IsActive);

        return Result.Success;
    }
}
