using Authorization.Application.Common.Interfaces;
using Authorization.Domain;
using Common.Abstractions.Providers;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Authorization.Application.Accounts.Refresh;

public sealed class RefreshCommandHandler
    : IRequestHandler<RefreshCommand, ErrorOr<RefreshResult>>
{
    private static readonly Error InvalidToken = Error.Unauthorized(
        "Account.InvalidRefreshToken",
        "Refresh token is invalid.");

    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ITokenHashGenerator _tokenHashGenerator;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly IGuidProvider _guidProvider;
    private readonly ILogger<RefreshCommandHandler> _logger;

    public RefreshCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IAccountRepository accountRepository,
        ITokenHashGenerator tokenHashGenerator,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        IGuidProvider guidProvider,
        ILogger<RefreshCommandHandler> logger)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _accountRepository = accountRepository;
        _tokenHashGenerator = tokenHashGenerator;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _guidProvider = guidProvider;
        _logger = logger;
    }

    public async Task<ErrorOr<RefreshResult>> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _tokenHashGenerator.Hash(request.RefreshToken);

        var storedToken = await _refreshTokenRepository.GetByHashAsync(tokenHash, cancellationToken);
        if (storedToken is null)
        {
            _logger.LogWarning("Refresh failed: no refresh token matches the presented value");

            return InvalidToken;
        }

        var now = _timeProvider.GetUtcNow();

        if (storedToken.IsRevoked)
        {
            _logger.LogWarning(
                "Refresh token reuse detected for account {AccountId}; revoking all active refresh tokens",
                storedToken.AccountId);

            await _refreshTokenRepository.RevokeAllActiveForAccountAsync(storedToken.AccountId, now, cancellationToken);

            return InvalidToken;
        }

        if (storedToken.IsExpired(now))
        {
            _logger.LogWarning("Refresh failed for account {AccountId}: refresh token expired", storedToken.AccountId);

            return InvalidToken;
        }

        var account = await _accountRepository.GetByIdAsync(storedToken.AccountId, cancellationToken);
        if (account is null)
        {
            _logger.LogWarning("Refresh failed: account {AccountId} no longer exists", storedToken.AccountId);

            return InvalidToken;
        }

        var canSignIn = account.EnsureCanSignIn();
        if (canSignIn.IsError)
        {
            _logger.LogWarning("Refresh blocked for account {AccountId}: {@Errors}", account.Id, canSignIn.Errors);

            return InvalidToken;
        }

        var newTokenDescriptor = _refreshTokenGenerator.Generate(now);

        var newTokenResult = RefreshToken.Issue(
            _guidProvider.NewGuid(),
            account.Id,
            newTokenDescriptor.TokenHash,
            now,
            newTokenDescriptor.ExpiresAt);

        if (newTokenResult.IsError)
            return newTokenResult.Errors;

        var revokeResult = storedToken.Revoke(now, newTokenResult.Value.Id);
        if (revokeResult.IsError)
            return revokeResult.Errors;

        var accessToken = _jwtTokenGenerator.Generate(account);

        await _refreshTokenRepository.AddAsync(newTokenResult.Value, cancellationToken);
        await _refreshTokenRepository.UpdateAsync(storedToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Refreshed tokens for account {AccountId}", account.Id);

        return new RefreshResult(
            account.Id,
            accessToken.Value,
            accessToken.ExpiresAt,
            newTokenDescriptor.Token,
            newTokenDescriptor.ExpiresAt);
    }
}
