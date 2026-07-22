using Authorization.Application.Common.Interfaces;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Authorization.Application.Accounts.ConfirmEmail;

public sealed class ConfirmEmailCommandHandler
    : IRequestHandler<ConfirmEmailCommand, ErrorOr<Guid>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITokenHashGenerator _tokenHashGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<ConfirmEmailCommandHandler> _logger;

    public ConfirmEmailCommandHandler(
        IAccountRepository accountRepository,
        ITokenHashGenerator tokenHashGenerator,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        ILogger<ConfirmEmailCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _tokenHashGenerator = tokenHashGenerator;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async Task<ErrorOr<Guid>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _tokenHashGenerator.Hash(request.Token);

        var account = await _accountRepository.GetByEmailConfirmationTokenHashAsync(tokenHash, cancellationToken);
        if (account is null)
        {
            _logger.LogWarning("Email confirmation failed: no account matches the presented token");

            return Error.NotFound("Account.ConfirmationTokenInvalid", "Confirmation link is invalid.");
        }

        var now = _timeProvider.GetUtcNow();

        var result = account.ConfirmEmail(tokenHash, now, account.Id);
        if (result.IsError)
        {
            _logger.LogWarning(
                "Email confirmation failed for account {AccountId}: {@Errors}",
                account.Id,
                result.Errors);

            return result.Errors;
        }

        await _accountRepository.UpdateAsync(account, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Account {AccountId} confirmed email", account.Id);

        return account.Id;
    }
}
