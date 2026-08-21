using Authorization.Application.Common.Interfaces;
using Authorization.Domain;
using Common.Abstractions.Providers;
using Common.Abstractions.Security;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Authorization.Application.Accounts.ChangePassword;

public sealed class ChangePasswordCommandHandler
    : IRequestHandler<ChangePasswordCommand, ErrorOr<ChangePasswordResult>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly TimeProvider _timeProvider;
    private readonly IGuidProvider _guidProvider;
    private readonly ILogger<ChangePasswordCommandHandler> _logger;

    public ChangePasswordCommandHandler(
        IAccountRepository accountRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IUnitOfWork unitOfWork,
        ICurrentUserProvider currentUserProvider,
        TimeProvider timeProvider,
        IGuidProvider guidProvider,
        ILogger<ChangePasswordCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _unitOfWork = unitOfWork;
        _currentUserProvider = currentUserProvider;
        _timeProvider = timeProvider;
        _guidProvider = guidProvider;
        _logger = logger;
    }

    public async Task<ErrorOr<ChangePasswordResult>> Handle(
        ChangePasswordCommand request,
        CancellationToken cancellationToken)
    {
        var accountId = _currentUserProvider.User?.Id;
        if (accountId is null)
            return Error.Unauthorized("Auth.Unauthenticated", "Authentication is required.");

        var account = await _accountRepository.GetByIdAsync(accountId.Value, cancellationToken);
        if (account is null)
            return Error.Unauthorized("Auth.Unauthenticated", "Authentication is required.");

        var canSignIn = account.EnsureCanSignIn();
        if (canSignIn.IsError)
            return canSignIn.Errors;

        if (!_passwordHasher.Verify(request.CurrentPassword, account.PasswordHash))
        {
            _logger.LogWarning("Password change rejected for account {AccountId}: wrong current password", account.Id);

            return Error.Validation("Account.CurrentPassword", "Current password is incorrect.");
        }

        if (_passwordHasher.Verify(request.NewPassword, account.PasswordHash))
            return Error.Validation("Account.NewPassword", "New password must differ from the current one.");

        var expectedVersion = account.Version;
        var now = _timeProvider.GetUtcNow();

        var changeResult = account.ChangePassword(_passwordHasher.Hash(request.NewPassword), now);
        if (changeResult.IsError)
            return changeResult.Errors;

        await _accountRepository.UpdateAsync(account, expectedVersion, cancellationToken);

        await _refreshTokenRepository.RevokeAllActiveForAccountAsync(account.Id, now, cancellationToken);

        var accessToken = _jwtTokenGenerator.Generate(account);
        var refreshTokenDescriptor = _refreshTokenGenerator.Generate(now);

        var refreshTokenResult = RefreshToken.Issue(
            _guidProvider.NewGuid(),
            account.Id,
            refreshTokenDescriptor.TokenHash,
            now,
            refreshTokenDescriptor.ExpiresAt);

        if (refreshTokenResult.IsError)
            return refreshTokenResult.Errors;

        await _refreshTokenRepository.AddAsync(refreshTokenResult.Value, cancellationToken);

        if (!await _unitOfWork.TrySaveChangesAsync(cancellationToken))
            return Error.Conflict("Account.ConcurrencyConflict", "Account was modified by another operation. Please retry.");

        _logger.LogInformation(
            "Account {AccountId} changed its password; all other sessions were revoked",
            account.Id);

        return new ChangePasswordResult(
            account.Id,
            accessToken.Value,
            accessToken.ExpiresAt,
            refreshTokenDescriptor.Token,
            refreshTokenDescriptor.ExpiresAt);
    }
}
